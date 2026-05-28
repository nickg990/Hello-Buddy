using System.Net;
using System.Text;
using HelloBuddy.Contracts;

namespace HelloBuddy.Api.Services;

/// <summary>
/// Renders the programme preview HTML used as the PDF source. Inlined here
/// (rather than a Razor view) because the API is a minimal-API host and
/// does not pull in the Razor view engine. The markup mirrors
/// <c>Views/Programmes/Preview.cshtml</c> in the UI project.
/// </summary>
public static class ProgrammePdfTemplate
{
    public static string Render(ProgrammeVm vm)
    {
        var sb = new StringBuilder(8192);
        sb.Append("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"/><title>");
        sb.Append(Enc(vm.ProgrammeName)).Append(" — ").Append(Enc(vm.PetName));
        sb.Append("</title><style>");
        sb.Append("@page { size: A4; margin: 0; }");
        sb.Append("body { font-family: 'Segoe UI', Arial, sans-serif; color: #222; margin: 0; padding: 0; font-size: 11pt; line-height: 1.4; }");
        sb.Append(".container { padding: 0 4mm; }");
        sb.Append("h1 { margin: 0 0 4px 0; font-size: 20pt; }");
        sb.Append("h2 { margin: 16px 0 4px 0; font-size: 14pt; border-bottom: 1px solid #ccc; padding-bottom: 2px; }");
        sb.Append(".subhead { color: #666; margin-bottom: 12px; }");
        sb.Append(".meta { display: flex; gap: 24px; margin-bottom: 12px; }");
        sb.Append(".meta div { flex: 1; }");
        sb.Append("table { width: 100%; border-collapse: collapse; margin-top: 4px; }");
        sb.Append("th, td { text-align: left; padding: 6px 8px; border-bottom: 1px solid #eee; vertical-align: top; }");
        sb.Append("th { background: #f4f4f4; font-weight: 600; font-size: 10pt; }");
        sb.Append(".ex-title { font-weight: 600; }");
        sb.Append(".ex-obj { color: #666; font-size: 10pt; }");
        sb.Append(".session { break-inside: avoid; margin-bottom: 12px; }");
        sb.Append(".footer { color: #888; font-size: 9pt; margin-top: 24px; text-align: center; }");
        sb.Append("</style></head><body><div class=\"container\">");

        sb.Append("<h1>").Append(Enc(vm.ProgrammeName)).Append("</h1>");
        sb.Append("<div class=\"subhead\">").Append(Enc(vm.CaseTitle))
          .Append(" · Programme #").Append(vm.ProgrammeId)
          .Append(" · ").Append(Enc(vm.Status)).Append("</div>");

        sb.Append("<div class=\"meta\">");
        sb.Append("<div><strong>Patient:</strong> ").Append(Enc(vm.PetName)).Append("</div>");
        sb.Append("<div><strong>Owner:</strong> ").Append(Enc(vm.OwnerName)).Append("</div>");
        sb.Append("<div><strong>Starts:</strong> ").Append(vm.StartDate.ToString("yyyy-MM-dd"));
        if (vm.EndDate is { } end)
            sb.Append(" – ").Append(end.ToString("yyyy-MM-dd"));
        sb.Append("</div></div>");

        if (!string.IsNullOrWhiteSpace(vm.Notes))
            sb.Append("<p><strong>Notes:</strong> ").Append(Enc(vm.Notes)).Append("</p>");

        foreach (var session in vm.Sessions)
        {
            sb.Append("<div class=\"session\"><h2>Session ").Append(session.SortOrder)
              .Append(" &mdash; ").Append(Enc(session.Period)).Append("</h2>");
            if (!string.IsNullOrWhiteSpace(session.Objective))
                sb.Append("<div class=\"subhead\">").Append(Enc(session.Objective)).Append("</div>");
            sb.Append("<table><thead><tr><th style=\"width:40%\">Exercise</th><th style=\"width:10%\">Reps</th>");
            sb.Append("<th style=\"width:10%\">Sets</th><th style=\"width:12%\">Hold (s)</th><th>Notes</th></tr></thead><tbody>");
            foreach (var ex in session.Exercises)
            {
                sb.Append("<tr><td><div class=\"ex-title\">").Append(Enc(ex.ExerciseTitle)).Append("</div>");
                if (!string.IsNullOrWhiteSpace(ex.ObjectiveSummary))
                    sb.Append("<div class=\"ex-obj\">").Append(Enc(ex.ObjectiveSummary)).Append("</div>");
                sb.Append("</td><td>").Append(ex.Reps?.ToString() ?? "—").Append("</td>");
                sb.Append("<td>").Append(ex.Sets?.ToString() ?? "—").Append("</td>");
                sb.Append("<td>").Append(ex.HoldSeconds?.ToString() ?? "—").Append("</td>");
                sb.Append("<td>").Append(Enc(ex.Notes ?? "")).Append("</td></tr>");
            }
            sb.Append("</tbody></table></div>");
        }

        sb.Append("<div class=\"footer\">Generated ").Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'"))
          .Append(" · Hello Buddy Cloud Admin</div></div></body></html>");
        return sb.ToString();
    }

    private static string Enc(string s) => WebUtility.HtmlEncode(s);
}
