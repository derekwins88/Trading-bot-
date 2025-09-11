from __future__ import annotations
import pandas as pd
from dataclasses import dataclass, field
from .entropy_engine import atr, delta_phi, verdict_from_series
from .capsule_logger import trade_capsule, write_ndjson
from .risk import RiskParams, position_size, stops_targets
from .session import session_weight
from .execution import fill_trade  # NEW


@dataclass
class Params:
    risk: RiskParams = field(default_factory=RiskParams)
    atr_period: int = 14
    look_ahead_bars: int = 64  # simulate into the future this many bars


class EntropyStrategy:
    def __init__(self, symbol: str, params: Params, outdir: str = "artifacts", equity: float = 50000.0):
        self.symbol = symbol
        self.p = params
        self.outdir = outdir
        self.equity = equity

    def run(self, df: pd.DataFrame, hud: bool = False) -> dict:
        high, low, close = df["high"].values, df["low"].values, df["close"].values
        atr_vals = atr(high, low, close, self.p.atr_period)
        dphi = delta_phi(atr_vals, close)
        verdict = verdict_from_series(dphi)

        side = "wait"
        # collapse → trend-follow bias
        if verdict.glyph == "⟿":
            side = "long" if close[-1] > close[-20] else "short"

        # Enter on the *penultimate* bar so we can simulate forward on the last N bars
        if side != "wait" and len(close) >= 2:
            entry_idx = -2
            entry = float(close[entry_idx])

            ts = df.index[entry_idx] if isinstance(df.index, pd.DatetimeIndex) else None
            w = session_weight(ts) if ts is not None else 1.0
            size = max(1, int(position_size(self.equity, float(atr_vals[entry_idx]), entry, self.p.risk) * w))
            stop, target = stops_targets(entry, side, float(atr_vals[entry_idx]), self.p.risk)

            highs_next = high[entry_idx + 1 : entry_idx + 1 + self.p.look_ahead_bars]
            lows_next  = low[entry_idx + 1 : entry_idx + 1 + self.p.look_ahead_bars]

            exit_px, exit_reason, bars_held = fill_trade(
                entry, side, stop, target, highs_next, lows_next, max_bars=self.p.look_ahead_bars, stop_first=True
            )

            # R-multiple
            risk_per_unit = abs(entry - stop)
            if side == "long":
                r_mult = (exit_px - entry) / risk_per_unit if risk_per_unit > 0 else 0.0
            else:
                r_mult = (entry - exit_px) / risk_per_unit if risk_per_unit > 0 else 0.0

            cap = trade_capsule(
                self.symbol, side, entry, exit_px,
                {
                    "np_wall": verdict.np_wall,
                    "no_recovery": verdict.no_recovery,
                    "sat_like": verdict.sat_like,
                    "glyph": verdict.glyph,
                    "ΔΦ_last": verdict.delta_phi,
                    "size": size,
                    "stop": stop,
                    "target": target,
                    "exit_reason": exit_reason,
                    "bars_held": bars_held,
                    "R": r_mult,
                },
                str(df.index[entry_idx]),
                str(df.index[min(len(df.index)-1, entry_idx + bars_held)])
            )
            write_ndjson(f"{self.outdir}/trades.ndjson", cap)

        if hud:
            print(f"{self.symbol} ΔΦ={verdict.delta_phi:.3f} glyph={verdict.glyph} side={side}")
        return {"symbol": self.symbol, "glyph": verdict.glyph, "side": side, "delta_phi_last": verdict.delta_phi}

