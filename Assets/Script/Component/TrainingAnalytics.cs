using Firebase.Analytics;

public static class TrainingAnalytics
{
    private static class GA
    {
        public const string EV_ROUND_SUCCESS = "training_round_success";
        public const string EV_STAGE_SESSION_START = "training_stage_session_start";

        public const string P_STAGE = "stage";
        public const string P_ROUND = "round";
        public const string P_FAILURES = "failures";
        public const string P_TIME_TO_CORRECT = "time_to_correct_sec";
        public const string P_SESSION_ID = "session_id";
    }

    public static void LogStageSessionStart(int stage, string sessionId)
    {
        // Unity Editor에서는 Firebase 호출 안 함
#if UNITY_EDITOR
        return;
#else
        if (!FirebaseInitializer.IsReady) return;

        FirebaseAnalytics.LogEvent(
            GA.EV_STAGE_SESSION_START,
            new Parameter(GA.P_STAGE, stage),
            new Parameter(GA.P_SESSION_ID, sessionId)
        );
#endif
    }

    public static void LogRoundSuccess(
        int stage,
        int round,
        int failures,
        float timeToCorrectSec,
        string sessionId
    )
    {
        // Unity Editor에서는 Firebase 호출 안 함
#if UNITY_EDITOR
        return;
#else
        if (!FirebaseInitializer.IsReady) return;

        FirebaseAnalytics.LogEvent(
            GA.EV_ROUND_SUCCESS,
            new Parameter(GA.P_STAGE, stage),
            new Parameter(GA.P_ROUND, round),
            new Parameter(GA.P_FAILURES, failures),
            new Parameter(GA.P_TIME_TO_CORRECT, timeToCorrectSec),
            new Parameter(GA.P_SESSION_ID, sessionId)
        );
#endif
    }
}
