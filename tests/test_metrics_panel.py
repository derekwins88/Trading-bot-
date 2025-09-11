from src.metrics import compute_metrics


def test_metrics_from_R_only_capsules():
    trades = [
        {"verdict": {"R": 1.0}},
        {"verdict": {"R": -0.5}},
        {"verdict": {"R": 2.0}},
    ]
    m = compute_metrics(trades)
    assert m["count"] == 3
    assert abs(m["cum_R"] - 2.5) < 1e-9
    assert m["wins"] == 2 and m["losses"] == 1
    assert 0.0 <= m["win_rate"] <= 1.0

