import numpy as np
from src.entropy_engine import verdict_from_series

def test_verdict_shapes():
    # strictly decreasing → SAT-like (⚖)
    d1 = np.array([0.06, 0.05, 0.04, 0.03])
    v1 = verdict_from_series(d1)
    assert v1.sat_like and v1.glyph == "⚖"

    # spike above wall with no clean tail → collapse (⟿)
    d2 = np.array([0.01, 0.02, 0.12, 0.10, 0.08, 0.06])
    v2 = verdict_from_series(d2)
    assert v2.np_wall and v2.glyph == "⟿"
