/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using NCalc.Handlers;

namespace Dev2.MathOperations.NCalc.Functions
{
    /// <summary>
    /// Registers all financial NCalc function handlers.
    /// Mirrors the Infragistics UltraCalcFunction financial set for cross-platform parity.
    /// </summary>
    internal static class FinancialFunctions
    {
        internal static void Register(IDictionary<string, Action<FunctionArgs>> handlers)
        {
            handlers["SLN"]      = Sln;
            handlers["SYD"]      = Syd;
            handlers["DDB"]      = Ddb;
            handlers["DB"]       = Db;
            handlers["NPV"]      = Npv;
            handlers["FV"]       = Fv;
            handlers["PV"]       = Pv;
            handlers["PMT"]      = Pmt;
            handlers["NPER"]     = Nper;
            handlers["PPMT"]     = Ppmt;
            handlers["IPMT"]     = Ipmt;
            handlers["IRR"]      = Irr;
            handlers["RATE"]     = Rate;
            handlers["INTRATE"]  = IntRate;
            handlers["DOLLARFR"] = DollarFr;
            handlers["DOLLARDE"] = DollarDe;
        }

        // ── Straight-line depreciation ───────────────────────────────────────────────────

        private static void Sln(FunctionArgs args)
        {
            // SLN(cost, salvage, life)
            args.Parameters.RequireArgs(3, "SLN");
            var cost    = args.Parameters.D(0);
            var salvage = args.Parameters.D(1);
            var life    = args.Parameters.D(2);
            if (life == 0) throw new InvalidOperationException("SLN: life cannot be zero.");
            args.Result = (cost - salvage) / life;
        }

        // ── Sum-of-years'-digits depreciation ───────────────────────────────────────────

        private static void Syd(FunctionArgs args)
        {
            // SYD(cost, salvage, life, period)
            args.Parameters.RequireArgs(4, "SYD");
            var cost    = args.Parameters.D(0);
            var salvage = args.Parameters.D(1);
            var life    = args.Parameters.D(2);
            var per     = args.Parameters.D(3);
            args.Result = ((cost - salvage) * (life - per + 1) * 2) / (life * (life + 1));
        }

        // ── Double-declining balance ─────────────────────────────────────────────────────

        private static void Ddb(FunctionArgs args)
        {
            // DDB(cost, salvage, life, period[, factor])
            args.Parameters.RequireArgs(4, "DDB");
            var cost    = args.Parameters.D(0);
            var salvage = args.Parameters.D(1);
            var life    = args.Parameters.D(2);
            var per     = args.Parameters.D(3);
            var factor  = args.Parameters.Length >= 5 ? args.Parameters.D(4) : 2.0;

            var bookValue = cost;
            double dep = 0;
            for (var i = 1; i <= per; i++)
            {
                dep = Math.Min((bookValue - salvage), bookValue * factor / life);
                dep = Math.Max(dep, 0);
                bookValue -= dep;
            }
            args.Result = dep;
        }

        // ── Fixed-declining balance ──────────────────────────────────────────────────────

        private static void Db(FunctionArgs args)
        {
            // DB(cost, salvage, life, period[, month])
            args.Parameters.RequireArgs(4, "DB");
            var cost    = args.Parameters.D(0);
            var salvage = args.Parameters.D(1);
            var life    = args.Parameters.D(2);
            var per     = args.Parameters.I32(3);
            var month   = args.Parameters.Length >= 5 ? args.Parameters.I32(4) : 12;

            if (cost == 0 || life == 0) { args.Result = 0d; return; }

            var rate = 1 - Math.Pow(salvage / cost, 1.0 / life);
            rate = Math.Round(rate, 3, MidpointRounding.AwayFromZero);

            var bookValue = cost;
            double dep = 0;
            for (var i = 1; i <= per; i++)
            {
                dep = i == 1 ? cost * rate * month / 12
                    : i == life + 1 ? (bookValue - salvage)
                    : bookValue * rate;
                bookValue -= dep;
            }
            args.Result = dep;
        }

        // ── Net present value ────────────────────────────────────────────────────────────

        private static void Npv(FunctionArgs args)
        {
            // NPV(rate, value1, value2, ...)
            args.Parameters.RequireArgs(2, "NPV");
            var rate   = args.Parameters.D(0);
            var result = 0.0;
            for (var i = 1; i < args.Parameters.Length; i++)
                result += args.Parameters.D(i) / Math.Pow(1 + rate, i);
            args.Result = result;
        }

        // ── Future value ────────────────────────────────────────────────────────────────

        private static void Fv(FunctionArgs args)
        {
            // FV(rate, nper, pmt[, pv[, type]])
            args.Parameters.RequireArgs(3, "FV");
            var rate = args.Parameters.D(0);
            var nper = args.Parameters.D(1);
            var pmt  = args.Parameters.D(2);
            var pv   = args.Parameters.Length >= 4 ? args.Parameters.D(3) : 0.0;
            var type = args.Parameters.Length >= 5 ? args.Parameters.D(4) : 0.0;

            args.Result = rate == 0
                ? -(pv + pmt * nper)
                : -(pv * Math.Pow(1 + rate, nper) +
                    pmt * (1 + rate * type) * (Math.Pow(1 + rate, nper) - 1) / rate);
        }

        // ── Present value ────────────────────────────────────────────────────────────────

        private static void Pv(FunctionArgs args)
        {
            // PV(rate, nper, pmt[, fv[, type]])
            args.Parameters.RequireArgs(3, "PV");
            var rate = args.Parameters.D(0);
            var nper = args.Parameters.D(1);
            var pmt  = args.Parameters.D(2);
            var fv   = args.Parameters.Length >= 4 ? args.Parameters.D(3) : 0.0;
            var type = args.Parameters.Length >= 5 ? args.Parameters.D(4) : 0.0;

            args.Result = rate == 0
                ? -(fv + pmt * nper)
                : -(fv / Math.Pow(1 + rate, nper) +
                    pmt * (1 + rate * type) * (1 - Math.Pow(1 + rate, -nper)) / rate);
        }

        // ── Payment ─────────────────────────────────────────────────────────────────────

        private static void Pmt(FunctionArgs args)
        {
            // PMT(rate, nper, pv[, fv[, type]])
            args.Parameters.RequireArgs(3, "PMT");
            var rate = args.Parameters.D(0);
            var nper = args.Parameters.D(1);
            var pv   = args.Parameters.D(2);
            var fv   = args.Parameters.Length >= 4 ? args.Parameters.D(3) : 0.0;
            var type = args.Parameters.Length >= 5 ? args.Parameters.D(4) : 0.0;

            if (rate == 0)
            {
                args.Result = -(pv + fv) / nper;
                return;
            }
            var pvif = Math.Pow(1 + rate, nper);
            args.Result = -rate / ((1 + rate * type) * (pvif - 1)) * (pv * pvif + fv);
        }

        // ── Number of periods ───────────────────────────────────────────────────────────

        private static void Nper(FunctionArgs args)
        {
            // NPER(rate, pmt, pv[, fv[, type]])
            args.Parameters.RequireArgs(3, "NPER");
            var rate = args.Parameters.D(0);
            var pmt  = args.Parameters.D(1);
            var pv   = args.Parameters.D(2);
            var fv   = args.Parameters.Length >= 4 ? args.Parameters.D(3) : 0.0;
            var type = args.Parameters.Length >= 5 ? args.Parameters.D(4) : 0.0;

            if (rate == 0)
            {
                args.Result = pmt == 0 ? 0d : -(pv + fv) / pmt;
                return;
            }
            var num = pmt * (1 + rate * type) - fv * rate;
            var den = pmt * (1 + rate * type) + pv * rate;
            if (den == 0 || num / den <= 0)
                throw new InvalidOperationException("NPER: invalid arguments — no valid solution.");
            args.Result = Math.Log(num / den) / Math.Log(1 + rate);
        }

        // ── Principal payment ───────────────────────────────────────────────────────────

        private static void Ppmt(FunctionArgs args)
        {
            // PPMT(rate, per, nper, pv[, fv[, type]])
            args.Parameters.RequireArgs(4, "PPMT");
            var rate = args.Parameters.D(0);
            var per  = args.Parameters.I32(1);
            var nper = args.Parameters.D(2);
            var pv   = args.Parameters.D(3);
            var fv   = args.Parameters.Length >= 5 ? args.Parameters.D(4) : 0.0;
            var type = args.Parameters.Length >= 6 ? args.Parameters.D(5) : 0.0;

            var pmtVal  = CalcPmt(rate, nper, pv, fv, type);
            var iptVal  = CalcIpmt(rate, per, nper, pv, fv, type);
            args.Result = pmtVal - iptVal;
        }

        // ── Interest payment ───────────────────────────────────────────────────────────

        private static void Ipmt(FunctionArgs args)
        {
            // IPMT(rate, per, nper, pv[, fv[, type]])
            args.Parameters.RequireArgs(4, "IPMT");
            var rate = args.Parameters.D(0);
            var per  = args.Parameters.I32(1);
            var nper = args.Parameters.D(2);
            var pv   = args.Parameters.D(3);
            var fv   = args.Parameters.Length >= 5 ? args.Parameters.D(4) : 0.0;
            var type = args.Parameters.Length >= 6 ? args.Parameters.D(5) : 0.0;
            args.Result = CalcIpmt(rate, per, nper, pv, fv, type);
        }

        // ── Internal rate of return ─────────────────────────────────────────────────────

        private static void Irr(FunctionArgs args)
        {
            // IRR(values...[, guess]) — Newton-Raphson iteration
            args.Parameters.RequireArgs(1, "IRR");
            var allVals = args.Parameters.AllD();
            double[] cashFlows;
            double guess;
            if (allVals.Length >= 2 && IsLikelyGuess(allVals[^1], allVals))
            {
                cashFlows = allVals[..^1];
                guess = allVals[^1];
            }
            else
            {
                cashFlows = allVals;
                guess = 0.1;
            }

            var rate = guess;
            for (var iter = 0; iter < 100; iter++)
            {
                var npv  = 0.0;
                var dnpv = 0.0;
                for (var i = 0; i < cashFlows.Length; i++)
                {
                    npv  += cashFlows[i] / Math.Pow(1 + rate, i);
                    dnpv -= i * cashFlows[i] / Math.Pow(1 + rate, i + 1);
                }
                if (Math.Abs(dnpv) < 1e-12) break;
                var newRate = rate - npv / dnpv;
                if (Math.Abs(newRate - rate) < 1e-10) { rate = newRate; break; }
                rate = newRate;
            }
            args.Result = rate;
        }

        // ── Rate ────────────────────────────────────────────────────────────────────────

        private static void Rate(FunctionArgs args)
        {
            // RATE(nper, pmt, pv[, fv[, type[, guess]]])
            args.Parameters.RequireArgs(3, "RATE");
            var nper  = args.Parameters.D(0);
            var pmt   = args.Parameters.D(1);
            var pv    = args.Parameters.D(2);
            var fv    = args.Parameters.Length >= 4 ? args.Parameters.D(3) : 0.0;
            var type  = args.Parameters.Length >= 5 ? args.Parameters.D(4) : 0.0;
            var guess = args.Parameters.Length >= 6 ? args.Parameters.D(5) : 0.1;

            var rate = guess;
            for (var iter = 0; iter < 300; iter++)
            {
                var pvif  = Math.Pow(1 + rate, nper);
                var f     = pv * pvif + pmt * (1 + rate * type) * (pvif - 1) / rate + fv;
                var df    = pv * nper * Math.Pow(1 + rate, nper - 1)
                          + pmt * type * (pvif - 1) / rate
                          + pmt * (1 + rate * type) * nper * Math.Pow(1 + rate, nper - 1) / rate
                          - pmt * (1 + rate * type) * (pvif - 1) / (rate * rate);
                if (Math.Abs(df) < 1e-12) break;
                var newRate = rate - f / df;
                if (Math.Abs(newRate - rate) < 1e-10) { rate = newRate; break; }
                rate = newRate;
            }
            args.Result = rate;
        }

        // ── Interest rate ───────────────────────────────────────────────────────────────

        private static void IntRate(FunctionArgs args)
        {
            // INTRATE(settlement, maturity, investment, redemption[, basis])
            args.Parameters.RequireArgs(4, "INTRATE");
            var investment  = args.Parameters.D(2);
            var redemption  = args.Parameters.D(3);
            var settle      = FunctionArgHelper.ToDate(args.Parameters[0].Evaluate());
            var maturity    = FunctionArgHelper.ToDate(args.Parameters[1].Evaluate());
            var days        = (maturity - settle).TotalDays;
            args.Result     = (redemption - investment) / investment / (days / 360.0);
        }

        // ── Dollar fraction helpers ──────────────────────────────────────────────────────

        private static void DollarFr(FunctionArgs args)
        {
            // DOLLARFR(decimal_dollar, fraction)
            args.Parameters.RequireArgs(2, "DOLLARFR");
            var dollar   = args.Parameters.D(0);
            var fraction = args.Parameters.I32(1);
            if (fraction <= 0) throw new InvalidOperationException("DOLLARFR: fraction must be positive.");
            var intPart  = Math.Truncate(dollar);
            var decPart  = dollar - intPart;
            args.Result  = intPart + decPart * fraction / Math.Pow(10, Math.Floor(Math.Log10(fraction) + 1));
        }

        private static void DollarDe(FunctionArgs args)
        {
            // DOLLARDE(fractional_dollar, fraction)
            args.Parameters.RequireArgs(2, "DOLLARDE");
            var dollar   = args.Parameters.D(0);
            var fraction = args.Parameters.I32(1);
            if (fraction <= 0) throw new InvalidOperationException("DOLLARDE: fraction must be positive.");
            var intPart  = Math.Truncate(dollar);
            var fracPart = dollar - intPart;
            args.Result  = intPart + fracPart / fraction * Math.Pow(10, Math.Floor(Math.Log10(fraction) + 1));
        }

        // ── Private helpers ─────────────────────────────────────────────────────────────

        private static double CalcPmt(double rate, double nper, double pv, double fv, double type)
        {
            if (rate == 0) return -(pv + fv) / nper;
            var pvif = Math.Pow(1 + rate, nper);
            return -rate / ((1 + rate * type) * (pvif - 1)) * (pv * pvif + fv);
        }

        private static double CalcIpmt(double rate, int per, double nper, double pv, double fv, double type)
        {
            if (per < 1 || per > nper)
                throw new InvalidOperationException($"IPMT: period {per} is out of range.");
            var pmtVal = CalcPmt(rate, nper, pv, fv, type);
            if (type == 1 && per == 1) return 0.0;
            var balance = pv;
            for (var i = 1; i < per; i++)
                balance = balance * (1 + rate) + pmtVal;
            return balance * rate;
        }

        private static bool IsLikelyGuess(double lastValue, double[] allValues)
            => allValues.Length > 1 && Math.Abs(lastValue) < 1.0 && allValues.Length <= allValues.Length;
    }
}