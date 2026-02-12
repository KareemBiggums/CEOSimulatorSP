using System;
using System.Collections.Generic;
using System.Linq;
using ExecutiveTycoon.Data;
using ExecutiveTycoon.Models;

namespace ExecutiveTycoon.Services;

public sealed class OfficeManager
{
    private readonly CompanyState _state;

    public OfficeManager(CompanyState state)
    {
        _state = state;
    }

    public IReadOnlyList<OfficeDefinition> Definitions => OfficeCatalog.All;

    public OfficeDefinition GetActiveHqDefinition() => Definitions.FirstOrDefault(o => o.Id == _state.ActiveHqOfficeId);

    public OfficeState GetState(string officeId) => _state.Offices.FirstOrDefault(x => x.OfficeId == officeId);

    public bool IsOwned(string officeId) => _state.Offices.Any(x => x.OfficeId == officeId && x.Owned);

    public bool TryPurchase(OfficeDefinition def, PaymentMode paymentMode, decimal companyFundsAvailable, int personalFunds, out decimal companyCost, out int personalCost)
    {
        companyCost = 0;
        personalCost = 0;

        var existing = GetState(def.Id);
        if (existing is { Owned: true })
        {
            return false;
        }

        var price = def.BasePrice;
        switch (paymentMode)
        {
            case PaymentMode.PersonalOnly:
                personalCost = price;
                break;
            case PaymentMode.CompanyOnly:
                companyCost = price;
                break;
            case PaymentMode.Split:
                personalCost = price / 2;
                companyCost = price - personalCost;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(paymentMode), paymentMode, null);
        }

        if (personalFunds < personalCost || companyFundsAvailable < companyCost)
        {
            return false;
        }

        if (existing == null)
        {
            _state.Offices.Add(new OfficeState
            {
                OfficeId = def.Id,
                Owned = true,
                UpgradeTier = 0,
                DecorTier = 0,
                StaffCapacityBonus = def.StaffCapacityBonus,
                OperatingCostModifier = def.OperatingCostModifier
            });
        }
        else
        {
            existing.Owned = true;
            existing.StaffCapacityBonus = def.StaffCapacityBonus;
            existing.OperatingCostModifier = def.OperatingCostModifier;
        }

        if (string.IsNullOrWhiteSpace(_state.ActiveHqOfficeId))
        {
            _state.ActiveHqOfficeId = def.Id;
        }

        return true;
    }

    public bool TryActivateHq(string officeId)
    {
        if (!IsOwned(officeId))
        {
            return false;
        }

        _state.ActiveHqOfficeId = officeId;
        return true;
    }

    public bool TryUpgradeOffice(string officeId, decimal companyFundsAvailable, int personalFunds, PaymentMode paymentMode, out decimal companyCost, out int personalCost)
    {
        companyCost = 0;
        personalCost = 0;

        var office = GetState(officeId);
        if (office == null || office.UpgradeTier >= 3)
        {
            return false;
        }

        var def = Definitions.FirstOrDefault(x => x.Id == officeId);
        if (def == null)
        {
            return false;
        }

        var cost = (int)(def.BasePrice * (0.15f + (office.UpgradeTier * 0.05f)));
        if (paymentMode == PaymentMode.Split)
        {
            personalCost = cost / 2;
            companyCost = cost - personalCost;
        }
        else if (paymentMode == PaymentMode.CompanyOnly)
        {
            companyCost = cost;
        }
        else
        {
            personalCost = cost;
        }

        if (personalFunds < personalCost || companyFundsAvailable < companyCost)
        {
            return false;
        }

        office.UpgradeTier++;
        office.StaffCapacityBonus = def.StaffCapacityBonus + office.UpgradeTier;
        office.OperatingCostModifier = Math.Max(0.82f, def.OperatingCostModifier - (office.UpgradeTier * 0.02f));
        return true;
    }
}
