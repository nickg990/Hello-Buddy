using System.Globalization;

namespace Canine_Physio_App.Services
{
    /// <summary>
    /// Manages session state (skip-to-next-session) using Preferences.
    /// Tracks the active session date/period and handles skip logic
    /// including auto-expiry when the calendar catches up.
    /// </summary>
    public class SessionStateService
    {
        private const string SkipActiveDateKey = "SkipActiveDate";
        private const string SkipActivePeriodKey = "SkipActivePeriod";
        private const string SkipOriginDateKey = "SkipOriginDate";

        /// <summary>
        /// The exercise key currently being viewed on ExerciseDetailPage.
        /// Set when the user opens an exercise; read by SkipToNextSessionPage
        /// to determine whether to skip an exercise or a whole session.
        /// Cleared when returning to MainExercisesPage.
        /// </summary>
        public string? CurrentExerciseKey { get; set; }

        /// <summary>
        /// Gets the current session context: the effective date and period
        /// the user should be working on, accounting for any active skip.
        /// </summary>
        /// <param name="dailySessions">The programme's daily sessions (1 or 2).</param>
        /// <returns>The resolved session date and period.</returns>
        public (DateTime Date, string Period) GetActiveSession(List<Models.DailySession> dailySessions)
        {
            ArgumentNullException.ThrowIfNull(dailySessions);
            var today = DateTime.Today;

            // Check for active skip
            var originDateStr = Preferences.Get(SkipOriginDateKey, string.Empty);

            if (!string.IsNullOrEmpty(originDateStr)
                && DateTime.TryParse(originDateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var originDate))
            {
                // Skip was made on a previous day — calendar caught up, clear it
                if (originDate < today)
                {
                    ClearSkip();
                    // Fall through to normal resolution
                }
                else
                {
                    // Skip is still active (made today)
                    var activeDateStr = Preferences.Get(SkipActiveDateKey, string.Empty);
                    var activePeriod = Preferences.Get(SkipActivePeriodKey, string.Empty);

                    if (!string.IsNullOrEmpty(activeDateStr)
                        && DateTime.TryParse(activeDateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var activeDate))
                    {
                        return (activeDate, activePeriod);
                    }
                }
            }

            // No skip — resolve naturally
            return (today, ResolveCurrentPeriod(dailySessions));
        }

        /// <summary>
        /// Whether a skip is currently active (made today, targeting today or tomorrow).
        /// </summary>
        public bool IsSkipActive
        {
            get
            {
                var originDateStr = Preferences.Get(SkipOriginDateKey, string.Empty);
                if (string.IsNullOrEmpty(originDateStr))
                    return false;

                return DateTime.TryParse(originDateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var originDate)
                    && originDate >= DateTime.Today;
            }
        }

        /// <summary>
        /// Attempts to skip to the next session.
        /// Returns the result of the skip attempt.
        /// </summary>
        /// <param name="currentDate">The current active session date.</param>
        /// <param name="currentPeriod">The current active session period.</param>
        /// <param name="dailySessions">The programme's daily sessions.</param>
        /// <returns>Result indicating success or the reason for blocking.</returns>
        public SkipResult TrySkip(DateTime currentDate, string currentPeriod,
            List<Models.DailySession> dailySessions)
        {
            ArgumentNullException.ThrowIfNull(dailySessions);
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var isMultiSession = dailySessions.Count > 1;

            // Already targeting tomorrow — block further skip
            if (currentDate > today)
            {
                return SkipResult.BlockedTomorrow;
            }

            if (isMultiSession)
            {
                if (currentPeriod.Equals("AM", StringComparison.OrdinalIgnoreCase))
                {
                    // AM → PM (same day)
                    SetSkip(today, today, "PM");
                    return SkipResult.SkippedToSession(today, "PM");
                }
                else
                {
                    // PM → tomorrow AM
                    SetSkip(today, tomorrow, "AM");
                    return SkipResult.SkippedToSession(tomorrow, "AM");
                }
            }
            else
            {
                // Single session → tomorrow (same period, could be blank)
                var period = dailySessions.Count > 0 ? dailySessions[0].Period : string.Empty;
                SetSkip(today, tomorrow, period);
                return SkipResult.SkippedToSession(tomorrow, period);
            }
        }

        /// <summary>
        /// Reverts an active skip, restoring the session to today's first period.
        /// Persists the reverted state so it survives navigation (e.g., returning
        /// from ExerciseDetailPage won't re-resolve via time-of-day).
        /// </summary>
        /// <param name="dailySessions">The programme's daily sessions.</param>
        /// <returns>The restored session date and period.</returns>
        public (DateTime Date, string Period) RevertSkip(List<Models.DailySession> dailySessions)
        {
            ArgumentNullException.ThrowIfNull(dailySessions);
            var today = DateTime.Today;
            var period = dailySessions.Count > 0 ? dailySessions[0].Period : string.Empty;

            // Persist as a skip targeting today's first session so GetActiveSession
            // returns it consistently until auto-expiry clears it tomorrow.
            SetSkip(today, today, period);
            return (today, period);
        }

        /// <summary>
        /// Clears all skip state from Preferences.
        /// Called when content is replaced or skip naturally expires.
        /// </summary>
        public void ClearSkip()
        {
            Preferences.Remove(SkipActiveDateKey);
            Preferences.Remove(SkipActivePeriodKey);
            Preferences.Remove(SkipOriginDateKey);
        }

        /// <summary>
        /// Resolves the current period for today.
        /// Always returns the first session (AM) so the user must complete or
        /// skip AM before PM loads, even if they open the app in the afternoon.
        /// Auto-advance handles AM → PM progression when all AM exercises are done.
        /// </summary>
        private static string ResolveCurrentPeriod(List<Models.DailySession> dailySessions)
        {
            return dailySessions.Count > 0 ? dailySessions[0].Period : string.Empty;
        }

        private void SetSkip(DateTime originDate, DateTime targetDate, string targetPeriod)
        {
            Preferences.Set(SkipOriginDateKey, originDate.ToString("O"));
            Preferences.Set(SkipActiveDateKey, targetDate.ToString("O"));
            Preferences.Set(SkipActivePeriodKey, targetPeriod);
        }
    }

    /// <summary>
    /// Result of a skip attempt.
    /// </summary>
    public class SkipResult
    {
        public bool IsBlocked { get; init; }
        public DateTime TargetDate { get; init; }
        public string TargetPeriod { get; init; } = string.Empty;

        /// <summary>Skip was blocked because the target is already tomorrow.</summary>
        public static SkipResult BlockedTomorrow => new() { IsBlocked = true };

        /// <summary>Skip succeeded to the given date/period.</summary>
        public static SkipResult SkippedToSession(DateTime date, string period) =>
            new() { IsBlocked = false, TargetDate = date, TargetPeriod = period };
    }
}
