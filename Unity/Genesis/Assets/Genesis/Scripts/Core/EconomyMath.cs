using System;
using System.Globalization;
using System.Collections.Generic;

namespace Genesis
{
    public readonly struct BulkPurchase
    {
        public readonly long Amount;
        public readonly double Cost;
        public readonly bool CanAfford;
        public BulkPurchase(long amount, double cost, bool canAfford)
        { Amount = amount; Cost = cost; CanAfford = canAfford; }
    }

    public static class EconomyMath
    {
        // Calendar pacing at 1x; VIP codes scale simulated time, not purchase costs.
        public const double CampaignProductionRate = 0.01;
        public static bool IsFinite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }

        public static BulkPurchase Bulk(GeneratorDefinition generator, long owned, double balance, int mode, double discount)
        {
            if (owned < 0 || !IsFinite(balance) || balance < 0 || discount < 0.5 || discount > 1 ||
                (mode != 1 && mode != 10 && mode != 100 && mode != -1))
                throw new ArgumentOutOfRangeException(nameof(mode), "Invalid purchase state.");
            double rate = generator.cost_mult;
            double first = generator.base_cost * discount * Math.Pow(rate, owned);
            long amount = mode;
            if (mode == -1)
            {
                double max = balance < first ? 1 : Math.Floor(Math.Log(1 + balance * (rate - 1) / first) / Math.Log(rate));
                amount = IsFinite(max) && max < long.MaxValue ? Math.Max(1L, (long)max) : 1;
            }
            double cost = amount == 1 ? first : first * (Math.Pow(rate, amount) - 1) / (rate - 1);
            return new BulkPurchase(amount, cost, IsFinite(cost) && cost > 0 && balance >= cost && owned <= long.MaxValue - amount);
        }

        public static double Production(GeneratorDefinition generator, long count, double multiplier)
        { return count <= 0 ? 0 : generator.base_prod * count * Math.Pow(2, count / 25) * multiplier * CampaignProductionRate; }

        public static string Format(double value, IList<string> suffixes)
        {
            if (!IsFinite(value) || value < 0) return "0";
            CultureInfo culture = CultureInfo.InvariantCulture;
            if (value > 0 && value < 0.001) return value.ToString("0.###E+0", culture);
            if (value > 0 && value < 1) return value.ToString("0.000", culture);
            if (value < 1000)
                return value < 10 && value % 1 > 0.05 ? value.ToString("F1", culture) : Math.Floor(value).ToString("0", culture);
            int index = (int)Math.Floor(Math.Log(value) / Math.Log(1000));
            if (index < suffixes.Count)
                return (value / Math.Pow(1000, index)).ToString("F2", culture) + " " + suffixes[index];
            int exponent = (int)Math.Floor(Math.Log(value) / Math.Log(10));
            return (value / Math.Pow(10, exponent)).ToString("F2", culture) + "e" + exponent.ToString(culture);
        }

        public static string FormatRate(double value, IList<string> suffixes)
        {
            if (!IsFinite(value) || value < 0) return "0.000";
            return value == 0 || value >= 0.001 && value < 1
                ? value.ToString("0.000", CultureInfo.InvariantCulture) : Format(value, suffixes);
        }
    }
}