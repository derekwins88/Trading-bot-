from __future__ import annotations
import pandas as pd
from dataclasses import dataclass
from .entropy_engine import atr, delta_phi, verdict_from_series
from .capsule_logger import trade_capsule, write_ndjson

@dataclass
class Params:
    risk_pct: float = 0.016
    rr: float = 2.5
    atr_period: int = 14

class EntropyStrategy:
    def __init__(self, symbol: str, params: Params, outdir: str = "artifacts"):
        self.symbol = symbol
        self.p = params
        self.outdir = outdir

    def run(self, df: pd.DataFrame, hud: bool = False) -> dict:
        high, low, close = df["high"].values, df["low"].values, df["close"].values
        atr_vals = atr(high, low, close, self.p.atr_period)
        dphi = delta_phi(atr_vals, close)
        verdict = verdict_from_series(dphi)

        # naive toy rule: go with collapse (⟿) in last 20 bars trend direction
        side = "wait"
        if verdict.glyph == "⟿":
            side = "long" if close[-1] > close[-20] else "short"

        # one-bar “trade” for demo
        entry, exit_ = float(close[-2]), float(close[-1])
        if side != "wait":
            cap = trade_capsule(self.symbol, side, entry, exit_,
                                {"np_wall": verdict.np_wall, "no_recovery": verdict.no_recovery,
                                 "sat_like": verdict.sat_like, "glyph": verdict.glyph,
                                 "ΔΦ_last": verdict.delta_phi},
                                str(df.index[-2]), str(df.index[-1]))
            write_ndjson(f"{self.outdir}/trades.ndjson", cap)

        if hud:
            print(f"{self.symbol} ΔΦ_last={verdict.delta_phi:.3f} verdict={verdict.glyph} side={side}")

        return {
            "symbol": self.symbol,
            "glyph": verdict.glyph,
            "side": side,
            "delta_phi_last": verdict.delta_phi
        }
