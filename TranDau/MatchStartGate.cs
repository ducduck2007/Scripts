public static class MatchStartGate
{
    public static bool localGameStartReceived { get; private set; }
    public static bool bothPlayersReadyInSnapshot { get; private set; }

    public static void ResetGate()
    {
        localGameStartReceived = false;
        bothPlayersReadyInSnapshot = false;
    }

    public static void MarkLocalGameStart()
    {
        localGameStartReceived = true;
    }

    public static void MarkBothPlayersReady()
    {
        bothPlayersReadyInSnapshot = true;
        TryHideLoading();
    }

    public static void TryHideLoading()
    {
        if (!localGameStartReceived) return;
        if (!bothPlayersReadyInSnapshot) return;

        if (ThongBaoController.Instance == null) return;
        if (ThongBaoController.Instance.LoadVaoTran == null) return;

        // Cách an toàn nhất nếu bạn chưa chắc API của LoadVaoTran
        ThongBaoController.Instance.LoadVaoTran.gameObject.SetActive(false);
    }
}
