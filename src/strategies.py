from __future__ import annotations
import numpy as np
from typing import Literal
from .indicators import sma

Mode = Literal["collapse", "recovery"]

def collapse_side(close: np.ndarray, lookback: int = 20) -> str:
    if close.size <= lookback:
        return "wait"
    return "long" if close[-1] > close[-lookback] else "short"

def recovery_side(close: np.ndarray, atr_val: float, k: float = 1.0, ma_period: int = 20) -> str:
    """
    Mean-reversion toward SMA. If price is > k*ATR above SMA → short, below → long.
    """
    if close.size < ma_period:
        return "wait"
    ma = sma(close, ma_period)[-1]
    px = float(close[-1])
    if px >= ma + k * atr_val:
        return "short"
    if px <= ma - k * atr_val:
        return "long"
    return "wait"

def entry_side(mode: Mode, close: np.ndarray, atr_val: float, **kw) -> str:
    if mode == "collapse":
        lb = int(kw.get("lookback", 20))
        return collapse_side(close, lb)
    else:
        k = float(kw.get("k", 1.0))
        ma = int(kw.get("ma_period", 20))
        return recovery_side(close, atr_val, k=k, ma_period=ma)
