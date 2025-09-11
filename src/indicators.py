from __future__ import annotations
import numpy as np

def sma(x: np.ndarray, period: int) -> np.ndarray:
    period = max(1, min(period, x.size))
    k = np.ones(period) / period
    y = np.convolve(x, k, mode="same")
    # warmup stabilize
    y[:period] = np.maximum.accumulate(y[:period])
    return y
