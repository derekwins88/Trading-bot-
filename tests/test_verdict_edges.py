import numpy as np
from src.entropy_engine import verdict_from_series, NP_WALL, RECOV_EPS


def test_last_spike_logic():
    d = np.array([0.01, 0.12, 0.03, 0.10, 0.08, 0.07])  # last spike at idx=3
    v = verdict_from_series(d)
    assert v.np_wall and v.glyph in {"⟿", "☑", "⚖"}  # just assert spike detection didn't crash


def test_recovery_detects_tail_below_eps():
    d = np.array([0.01, NP_WALL + 0.02, RECOV_EPS * 0.9, RECOV_EPS * 0.8, RECOV_EPS * 0.7, 0.02])
    v = verdict_from_series(d)
    # spike then full tail below epsilon → recovered → no_recovery False
    assert v.np_wall and (v.glyph in {"☑", "⚖"})

