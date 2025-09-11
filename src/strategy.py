from __future__ import annotations
import pandas as pd
from dataclasses import dataclass
from .entropy_engine import atr, delta_phi, verdict_from_series
from .capsule_logger import trade_capsule, write_ndjson
from .risk import RiskParams, position_size, stops_targets
from .session import session_weight


@dataclass
class Params:
    risk: RiskParams = RiskParams()
    atr_period: int = 14


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
        if verdict.glyph == "⟿":  # collapse → trend-follow
            side = "long" if close[-1] > close[-20] else "short"

        entry = float(close[-1])
        if side != "wait":
            # session weighting on size
            ts = df.index[-1] if isinstance(df.index, pd.DatetimeIndex) else None
            w = session_weight(ts) if ts is not None else 1.0
            size = max(1, int(position_size(self.equity, float(atr_vals[-1]), entry, self.p.risk) * w))

            stop, target = stops_targets(entry, side, float(atr_vals[-1]), self.p.risk)

            # demo exit: mark-to-market at last close (replace with bar-by-bar in Phase 4)
            exit_px = float(close[-1])
            cap = trade_capsule(
                self.symbol, side, entry, exit_px,
                {
                    "np_wall": verdict.np_wall, "no_recovery": verdict.no_recovery,
                    "sat_like": verdict.sat_like, "glyph": verdict.glyph, "ΔΦ_last": verdict.delta_phi,
                    "size": size, "stop": stop, "target": target
                },
                str(df.index[-1]), str(df.index[-1])
            )
            write_ndjson(f"{self.outdir}/trades.ndjson", cap)

        if hud:
            print(f"{self.symbol} ΔΦ={verdict.delta_phi:.3f} glyph={verdict.glyph} side={side}")
        return {"symbol": self.symbol, "glyph": verdict.glyph, "side": side, "delta_phi_last": verdict.delta_phi}

