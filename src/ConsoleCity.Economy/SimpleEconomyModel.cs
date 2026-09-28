using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Economy;

public sealed class SimpleEconomyModel : IEconomyModel, IEconomyEngine

{
    public EconomySnapshot Snapshot { get; private set; }

    public IEconomyTransportBridge? TransportBridge { get; }

    private readonly List<GoodsInTransit> goodsInTransit = new();

    public SimpleEconomyModel(EconomySnapshot snapshot, IEconomyTransportBridge? transportBridge = null)
    {
        Snapshot = snapshot;
        TransportBridge = transportBridge;
    }

    public SimpleEconomyModel()
        : this(new EconomySnapshot(new SimulationTime(0), [], [], new EconomicIndicators(0m, Money.Zero, Money.Zero, Money.Zero, Money.Zero, 0)), null)
    {
    }

    public EconomyStepResult Advance(SimulationTime currentTime, IReadOnlyList<PersonAgent> people, IReadOnlyList<HouseholdAgent> households)
    {
        ArgumentNullException.ThrowIfNull(people);
        ArgumentNullException.ThrowIfNull(households);

        var events = Snapshot.Events.ToList();
        var transfers = new List<EconomicTransfer>();
        var peopleById = people.ToDictionary(person => person.Id);
        var householdsById = households.ToDictionary(household => household.Id);
        var businesses = Snapshot.Businesses.ToList();
        var prices = Snapshot.Prices.ToDictionary(price => price.ResourceId, price => price);
        var recipes = Snapshot.Recipes.ToDictionary(recipe => recipe.RecipeKey, recipe => recipe);

        // Process completed deliveries first: add goods to destination inventory.
        var deliveredGoods = new List<DeliveredGoods>();
        if (TransportBridge is not null)
        {
            // Use reflection to call GetCompletedDeliveries if available.
            var getCompletedMethod = TransportBridge.GetType().GetMethod("GetCompletedDeliveries");
            if (getCompletedMethod is not null)
            {
                var completed = (IReadOnlyList<DeliveredGoods>?)getCompletedMethod.Invoke(TransportBridge, null) ?? new List<DeliveredGoods>();
                foreach (var delivery in completed)
                {
                    deliveredGoods.Add(delivery);
                    if (delivery.Destination is not null)
                    {
                        var targetBusiness = businesses.FirstOrDefault(b => b.Location == delivery.Destination);
                        if (targetBusiness is not null)
                        {
                            var inventory = targetBusiness.Inventory.ToList();
                            AddResource(inventory, delivery.ResourceId, delivery.Quantity.Value);
                            var updatedBusiness = new BusinessModel(
                                targetBusiness.Id,
                                targetBusiness.Name,
                                targetBusiness.ActorType,
                                targetBusiness.State,
                                targetBusiness.FoundedAt,
                                targetBusiness.BuildingId,
                                targetBusiness.Location,
                                targetBusiness.Cash,
                                targetBusiness.Revenue,
                                targetBusiness.Expenses,
                                targetBusiness.WagePerEmployee,
                                targetBusiness.Employees,
                                inventory,
                                targetBusiness.RecipeKeys);
                            var businessIndex = businesses.IndexOf(targetBusiness);
                            businesses[businessIndex] = updatedBusiness;
                        }
                    }
                    goodsInTransit.RemoveAll(g => g.JourneyId == delivery.JourneyId);
                    events.Add(new EconomicEvent(
                        EconomicEventType.ConsumptionRecorded,
                        new SimulationTick(currentTime.Tick),
                        $"Delivery of {delivery.ResourceId.Value} arrived.",
                        0.1d,
                        resourceId: delivery.ResourceId,
                        amount: new Money(delivery.Quantity.Value)));
                }
            }
        }

        (businesses, peopleById, householdsById) = ProcessPayroll(currentTime, businesses, peopleById, householdsById, events, transfers);
        var transportRequests = new List<GoodsTransportRequest>();
        businesses = ProcessProduction(currentTime, businesses, recipes, prices, events, transportRequests, TransportBridge);
        businesses = ProcessConsumption(currentTime, businesses, prices, householdsById, events, transfers, transportRequests, TransportBridge);
        prices = UpdatePrices(currentTime, businesses, prices, recipes, householdsById, events);
        var government = UpdateGovernmentFinance(currentTime, businesses, events);
        var indicators = BuildIndicators(businesses, peopleById.Values, householdsById.Values, government);

        // Track new transport requests as in-transit goods.
        foreach (var req in transportRequests)
        {
            goodsInTransit.Add(new GoodsInTransit(req.Id, req.ResourceId, req.Quantity, req.To));
        }

        Snapshot = new EconomySnapshot(currentTime, prices.Values.ToList(), Snapshot.Recipes, indicators, businesses, government, events);
        return new EconomyStepResult(Snapshot, peopleById.Values.ToList(), householdsById.Values.ToList(), events, transfers, transportRequests, deliveredGoods);
    }

    private static (List<BusinessModel> Businesses, Dictionary<PersonId, PersonAgent> PeopleById, Dictionary<HouseholdId, HouseholdAgent> HouseholdsById) ProcessPayroll(
        SimulationTime currentTime,
        List<BusinessModel> businesses,
        Dictionary<PersonId, PersonAgent> peopleById,
        Dictionary<HouseholdId, HouseholdAgent> householdsById,
        List<EconomicEvent> events,
        List<EconomicTransfer> transfers)
    {
        for (var i = 0; i < businesses.Count; i++)
        {
            var business = businesses[i];
            if (business.State is BusinessLifecycleState.Closed)
            {
                continue;
            }

            var employeeCount = business.Employees.Count;
            if (employeeCount == 0 || business.WagePerEmployee.Amount <= 0m)
            {
                continue;
            }

            var totalWages = business.WagePerEmployee.Amount * employeeCount;
            var payout = Math.Min(business.Cash.Amount, totalWages);
            if (payout <= 0m)
            {
                events.Add(new EconomicEvent(EconomicEventType.ShortageDetected, new SimulationTick(currentTime.Tick), $"{business.Name} could not pay wages.", 0.7d, businessId: business.Id));
                continue;
            }

            var wagePerEmployee = payout / employeeCount;
            var newPeople = peopleById.ToDictionary(pair => pair.Key, pair => pair.Value);
            var newHouseholds = householdsById.ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var employeeId in business.Employees)
            {
                if (!newPeople.TryGetValue(employeeId, out var person))
                {
                    continue;
                }

                var wage = new Money(wagePerEmployee);
                var updatedPerson = new PersonAgent(
                    person.Id,
                    person.HouseholdId,
                    person.DisplayName,
                    person.Age,
                    person.LifeStage,
                    new EmploymentRecord(EmploymentState.Employed, business.Id, business.Location, wage),
                    person.Income + wage,
                    person.Expenses,
                    person.Balance + wage,
                    person.ResidencePlotId,
                    person.CurrentLocation,
                    AgentActivity.Working,
                    person.TransportPreference,
                    Math.Min(1d, person.Satisfaction + 0.01d),
                    person.Attributes,
                    person.Needs,
                    person.Skills,
                    person.Relationships,
                    person.Goals,
                    person.Routine);

                newPeople[employeeId] = updatedPerson;

                if (newHouseholds.TryGetValue(person.HouseholdId, out var household))
                {
                    newHouseholds[household.Id] = new HouseholdAgent(
                        household.Id,
                        household.HomePlotId,
                        household.HomeLocation,
                        household.Members,
                        household.Income + wage,
                        household.Expenses,
                        household.Savings + wage,
                        household.Debt,
                        household.FoodDemand,
                        household.UtilityDemand,
                        household.TransportAssets,
                        Math.Min(1d, household.Satisfaction + 0.01d));
                }

                transfers.Add(new EconomicTransfer(
                    EconomicTransferType.Wage,
                    wage,
                    $"{business.Name} paid wages.",
                    fromBusinessId: business.Id,
                    toPersonId: employeeId,
                    toHouseholdId: person.HouseholdId));
            }

            peopleById = newPeople;
            householdsById = newHouseholds;
            businesses[i] = new BusinessModel(
                business.Id,
                business.Name,
                business.ActorType,
                business.State,
                business.FoundedAt,
                business.BuildingId,
                business.Location,
                business.Cash - new Money(payout),
                business.Revenue,
                business.Expenses + new Money(payout),
                business.WagePerEmployee,
                business.Employees,
                business.Inventory,
                business.RecipeKeys);

            events.Add(new EconomicEvent(
                EconomicEventType.WagePaid,
                new SimulationTick(currentTime.Tick),
                $"{business.Name} paid wages.",
                0.1d,
                businessId: business.Id,
                amount: new Money(payout)));
        }

        return (businesses, peopleById, householdsById);
    }

    private static List<BusinessModel> ProcessProduction(
        SimulationTime currentTime,
        List<BusinessModel> businesses,
        IReadOnlyDictionary<string, ProductionRecipe> recipes,
        IReadOnlyDictionary<ResourceId, PriceQuote> prices,
        List<EconomicEvent> events,
        List<GoodsTransportRequest> transportRequests,
        IEconomyTransportBridge? transportBridge)
    {
        for (var i = 0; i < businesses.Count; i++)
        {
            var business = businesses[i];
            if (business.State is BusinessLifecycleState.Closed or BusinessLifecycleState.Insolvent)
            {
                continue;
            }

            var inventory = business.Inventory.ToList();
            var producedAny = false;

            foreach (var recipeKey in business.RecipeKeys)
            {
                if (!recipes.TryGetValue(recipeKey, out var recipe))
                {
                    continue;
                }

                if (!HasInputs(inventory, recipe.Inputs))
                {
                    // Identify missing inputs and request transport to replenish from external markets or suppliers.
                    foreach (var input in recipe.Inputs)
                    {
                        var available = inventory.Where(item => item.ResourceId == input.ResourceId).Sum(item => item.Quantity.Value);
                        var missing = Math.Max(0m, input.Quantity.Value - available);
                        if (missing <= 0m) continue;
                        var req = new GoodsTransportRequest(EntityId.New(), input.ResourceId, new Quantity(missing), null, business.Location);
                        transportRequests.Add(req);
                        transportBridge?.RequestTransport(req);
                        events.Add(new EconomicEvent(EconomicEventType.ShortageDetected, new SimulationTick(currentTime.Tick), $"{business.Name} lacks {input.ResourceId.Value} (need {missing}). Transport requested.", 0.5d, businessId: business.Id, resourceId: input.ResourceId));
                    }

                    continue;
                }

                ConsumeInputs(inventory, recipe.Inputs);
                ProduceOutputs(inventory, recipe.Outputs);
                producedAny = true;
                var outputValue = EstimateOutputValue(recipe.Outputs, prices);

                businesses[i] = new BusinessModel(
                    business.Id,
                    business.Name,
                    business.ActorType,
                    business.State,
                    business.FoundedAt,
                    business.BuildingId,
                    business.Location,
                    business.Cash + outputValue,
                    business.Revenue + outputValue,
                    business.Expenses,
                    business.WagePerEmployee,
                    business.Employees,
                    inventory,
                    business.RecipeKeys);

                events.Add(new EconomicEvent(
                    EconomicEventType.ProductionCompleted,
                    new SimulationTick(currentTime.Tick),
                    $"{business.Name} produced goods from {recipeKey}.",
                    0.2d,
                    businessId: business.Id,
                    amount: outputValue));
            }

            if (producedAny)
            {
                business = businesses[i];
            }
        }

        return businesses;
    }

    private static List<BusinessModel> ProcessConsumption(
        SimulationTime currentTime,
        List<BusinessModel> businesses,
        IReadOnlyDictionary<ResourceId, PriceQuote> prices,
        Dictionary<HouseholdId, HouseholdAgent> householdsById,
        List<EconomicEvent> events,
        List<EconomicTransfer> transfers,
        List<GoodsTransportRequest> transportRequests,
        IEconomyTransportBridge? transportBridge)
    {
        var foodQuote = prices.Values.FirstOrDefault(quote => IsFoodResource(quote.ResourceId));
        if (foodQuote is null)
        {
            return businesses;
        }

        var updatedBusinesses = businesses.ToList();
        foreach (var householdPair in householdsById.ToList())
        {
            var household = householdPair.Value;
            if (household.FoodDemand.Value <= 0m)
            {
                continue;
            }

            var businessIndex = updatedBusinesses.FindIndex(business => HasResource(business.Inventory, foodQuote.ResourceId));
            if (businessIndex < 0)
            {
                // No supplier has the resource: request transport from external market to the household.
                var req = new GoodsTransportRequest(EntityId.New(), foodQuote.ResourceId, new Quantity(household.FoodDemand.Value), null, household.HomeLocation);
                transportRequests.Add(req);
                transportBridge?.RequestTransport(req);
                events.Add(new EconomicEvent(EconomicEventType.ShortageDetected, new SimulationTick(currentTime.Tick), $"{household.Id} could not buy food. Transport requested.", 0.4d, householdId: household.Id, resourceId: foodQuote.ResourceId));
                continue;
            }

            var price = foodQuote.CurrentPrice.Amount;
            var purchaseQuantity = Math.Min(1m, household.FoodDemand.Value);
            var cost = new Money(price * purchaseQuantity);
            if (household.Savings.Amount < cost.Amount)
            {
                events.Add(new EconomicEvent(EconomicEventType.ShortageDetected, new SimulationTick(currentTime.Tick), $"{household.Id} could not afford food.", 0.4d, householdId: household.Id, resourceId: foodQuote.ResourceId, amount: cost));
                continue;
            }

            var updatedHousehold = new HouseholdAgent(
                household.Id,
                household.HomePlotId,
                household.HomeLocation,
                household.Members,
                household.Income,
                household.Expenses + cost,
                household.Savings - cost,
                household.Debt,
                ReduceQuantity(household.FoodDemand, purchaseQuantity),
                household.UtilityDemand,
                household.TransportAssets,
                Math.Min(1d, household.Satisfaction + 0.02d));

            householdsById[household.Id] = updatedHousehold;

            var business = updatedBusinesses[businessIndex];
            var inventory = business.Inventory.ToList();
            ConsumeResource(inventory, foodQuote.ResourceId, purchaseQuantity);
            updatedBusinesses[businessIndex] = new BusinessModel(
                business.Id,
                business.Name,
                business.ActorType,
                business.State,
                business.FoundedAt,
                business.BuildingId,
                business.Location,
                business.Cash + cost,
                business.Revenue + cost,
                business.Expenses,
                business.WagePerEmployee,
                business.Employees,
                inventory,
                business.RecipeKeys);

            events.Add(new EconomicEvent(
                EconomicEventType.ConsumptionRecorded,
                new SimulationTick(currentTime.Tick),
                $"Household {household.Id} bought food.",
                0.1d,
                householdId: household.Id,
                resourceId: foodQuote.ResourceId,
                amount: cost));
            transfers.Add(new EconomicTransfer(
                EconomicTransferType.HouseholdPurchase,
                cost,
                $"Household {household.Id} bought food.",
                fromBusinessId: business.Id,
                toHouseholdId: household.Id));
        }

        return updatedBusinesses;
    }

    private static Dictionary<ResourceId, PriceQuote> UpdatePrices(
        SimulationTime currentTime,
        IReadOnlyList<BusinessModel> businesses,
        IReadOnlyDictionary<ResourceId, PriceQuote> previousPrices,
        IReadOnlyDictionary<string, ProductionRecipe> recipes,
        IReadOnlyDictionary<HouseholdId, HouseholdAgent> householdsById,
        List<EconomicEvent> events)
    {
        var resources = new HashSet<ResourceId>(previousPrices.Keys);
        foreach (var business in businesses)
        {
            foreach (var item in business.Inventory)
            {
                resources.Add(item.ResourceId);
            }
        }

        var updatedPrices = new Dictionary<ResourceId, PriceQuote>();
        foreach (var resource in resources)
        {
            var supply = SumInventory(businesses, resource);
            var demand = SumRecipeDemand(recipes.Values, resource) + SumHouseholdDemand(householdsById.Values, resource);
            var previous = previousPrices.TryGetValue(resource, out var existing)
                ? existing
                : new PriceQuote(resource, new Money(1m), new Money(1m), Quantity.Zero, Quantity.Zero);
            var updated = previous.WithMarketState(supply, demand);
            updatedPrices[resource] = updated;

            if (updated.CurrentPrice != previous.CurrentPrice)
            {
                events.Add(new EconomicEvent(
                    EconomicEventType.PriceUpdated,
                    new SimulationTick(currentTime.Tick),
                    $"{resource.Value} price updated.",
                    0.05d,
                    resourceId: resource));
            }
        }

        return updatedPrices;
    }

    private static GovernmentFinance UpdateGovernmentFinance(SimulationTime currentTime, IReadOnlyList<BusinessModel> businesses, List<EconomicEvent> events)
    {
        var revenue = businesses.Sum(business => business.Revenue.Amount);
        var tax = new Money(revenue * 0.05m);
        var expenditure = new Money(25m + businesses.Count * 5m);
        events.Add(new EconomicEvent(EconomicEventType.TaxCollected, new SimulationTick(currentTime.Tick), "Government collected tax.", 0.05d, amount: tax));
        events.Add(new EconomicEvent(EconomicEventType.GovernmentSpending, new SimulationTick(currentTime.Tick), "Government spent on services.", 0.05d, amount: expenditure));
        return new GovernmentFinance(tax, expenditure, tax - expenditure);
    }

    private static EconomicIndicators BuildIndicators(
        IReadOnlyList<BusinessModel> businesses,
        IEnumerable<PersonAgent> people,
        IEnumerable<HouseholdAgent> households,
        GovernmentFinance government)
    {
        var peopleList = people.ToList();
        var householdsList = households.ToList();
        var employedPeople = peopleList.Count(person => person.EmploymentState == EmploymentState.Employed);
        var employmentRate = peopleList.Count == 0 ? 0m : (decimal)employedPeople / peopleList.Count;

        return new EconomicIndicators(
            employmentRate,
            new Money(householdsList.Sum(household => household.Income.Amount)),
            new Money(householdsList.Sum(household => household.Expenses.Amount)),
            new Money(businesses.Sum(business => business.Revenue.Amount)),
            new Money(businesses.Sum(business => business.Expenses.Amount)),
            businesses.Count(business => business.State is BusinessLifecycleState.Closed),
            government.Revenue,
            government.Expenditure,
            new Quantity(householdsList.Sum(household => household.FoodDemand.Value)),
            new Quantity(businesses.Sum(business => business.Inventory.Sum(item => item.Quantity.Value))),
            new Quantity(businesses.Sum(business => business.Inventory.Sum(item => item.Quantity.Value))),
            government.Balance);
    }

    private static bool HasInputs(IReadOnlyList<InventoryLine> inventory, IReadOnlyList<InventoryLine> inputs)
    {
        foreach (var input in inputs)
        {
            var available = inventory.Where(item => item.ResourceId == input.ResourceId).Sum(item => item.Quantity.Value);
            if (available < input.Quantity.Value)
            {
                return false;
            }
        }

        return true;
    }

    private static void ConsumeInputs(List<InventoryLine> inventory, IReadOnlyList<InventoryLine> inputs)
    {
        foreach (var input in inputs)
        {
            ConsumeResource(inventory, input.ResourceId, input.Quantity.Value);
        }
    }

    private static void ProduceOutputs(List<InventoryLine> inventory, IReadOnlyList<InventoryLine> outputs)
    {
        foreach (var output in outputs)
        {
            AddResource(inventory, output.ResourceId, output.Quantity.Value);
        }
    }

    private static Money EstimateOutputValue(IReadOnlyList<InventoryLine> outputs, IReadOnlyDictionary<ResourceId, PriceQuote> prices)
    {
        decimal total = 0m;
        foreach (var output in outputs)
        {
            var unitPrice = prices.TryGetValue(output.ResourceId, out var quote)
                ? quote.CurrentPrice.Amount
                : 1m;
            total += unitPrice * output.Quantity.Value;
        }

        return new Money(total);
    }

    private static bool HasResource(IReadOnlyList<InventoryLine> inventory, ResourceId resourceId)
        => inventory.Any(item => item.ResourceId == resourceId && item.Quantity.Value > 0m);

    private static Quantity SumInventory(IReadOnlyList<BusinessModel> businesses, ResourceId resourceId)
        => new(businesses.Sum(business => business.Inventory.Where(item => item.ResourceId == resourceId).Sum(item => item.Quantity.Value)));

    private static Quantity SumRecipeDemand(IEnumerable<ProductionRecipe> recipes, ResourceId resourceId)
        => new(recipes.Sum(recipe => recipe.Inputs.Where(item => item.ResourceId == resourceId).Sum(item => item.Quantity.Value)));

    private static Quantity SumHouseholdDemand(IEnumerable<HouseholdAgent> households, ResourceId resourceId)
    {
        if (!IsFoodResource(resourceId))
        {
            return Quantity.Zero;
        }

        return new Quantity(households.Sum(household => household.FoodDemand.Value));
    }

    private static bool IsFoodResource(ResourceId resourceId)
        => resourceId.Value.Contains("food", StringComparison.OrdinalIgnoreCase)
            || resourceId.Value.Contains("grain", StringComparison.OrdinalIgnoreCase)
            || resourceId.Value.Contains("meal", StringComparison.OrdinalIgnoreCase);

    private static void ConsumeResource(List<InventoryLine> inventory, ResourceId resourceId, decimal amount)
    {
        for (var i = 0; i < inventory.Count; i++)
        {
            var item = inventory[i];
            if (item.ResourceId != resourceId)
            {
                continue;
            }

            var remaining = item.Quantity.Value - amount;
            inventory[i] = remaining <= 0m
                ? new InventoryLine(resourceId, Quantity.Zero)
                : new InventoryLine(resourceId, new Quantity(remaining));
            return;
        }

        inventory.Add(new InventoryLine(resourceId, Quantity.Zero));
    }

    private static void AddResource(List<InventoryLine> inventory, ResourceId resourceId, decimal amount)
    {
        for (var i = 0; i < inventory.Count; i++)
        {
            var item = inventory[i];
            if (item.ResourceId != resourceId)
            {
                continue;
            }

            inventory[i] = new InventoryLine(resourceId, new Quantity(item.Quantity.Value + amount));
            return;
        }

        inventory.Add(new InventoryLine(resourceId, new Quantity(amount)));
    }

    private static Quantity ReduceQuantity(Quantity quantity, decimal amount)
        => quantity.Value <= amount ? Quantity.Zero : new Quantity(quantity.Value - amount);
}
