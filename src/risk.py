from __future__ import annotations
from dataclasses import dataclass
import math


@dataclass
class RiskParams:
    risk_pct: float = 0.016   # 1.6% of equity
    rr: float = 2.5           # reward:risk
    atr_mult: float = 1.5     # stop = ATR * atr_mult
    min_size: int = 1


def position_size(equity: float, atr_value: float, price: float, p: RiskParams) -> int:
    # Instrument-agnostic demo: $1 move = 1 PnL unit
    stop_dist = max(1e-6, atr_value * p.atr_mult)
    cash_risk = max(0.0, equity * p.risk_pct)
    size = math.floor(cash_risk / stop_dist)
    return max(p.min_size, size)


def stops_targets(entry: float, side: str, atr_value: float, p: RiskParams) -> tuple[float, float]:
    sdist = max(1e-6, atr_value * p.atr_mult)
    if side == "long":
        return entry - sdist, entry + sdist * p.rr
    elif side == "short":
        return entry + sdist, entry - sdist * p.rr
    return entry, entry

