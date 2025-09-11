from src.metrics import compute_metrics


def test_expectancy_panel():
    trades = [{"verdict": {"R": r}} for r in [1.0, 2.0, -1.0, -0.5]]
    m = compute_metrics(trades)
    assert round(m["cum_R"], 3) == 1.5
    assert m["wins"] == 2 and m["losses"] == 2
    assert m["avg_win_R"] > 0 and m["avg_loss_R"] < 0
    assert m["payoff_ratio"] > 0
