using System.Net;
using System.Text;
using MOVOQ.Application.Features.Dev;

namespace MOVOQ.Api.Hosting;

internal static class DevDashboardHtml
{
    internal static string Render(IReadOnlyList<DevUserDto> users)
    {
        var rows = new StringBuilder();
        if (users.Count == 0)
        {
            rows.Append("<tr><td colspan=\"8\" class=\"empty\">No users yet. Add a fake user or seed the sample set.</td></tr>");
        }
        else
        {
            foreach (var user in users)
            {
                rows.Append("<tr>")
                    .Append(Td(user.DisplayName ?? "—"))
                    .Append(Td(user.Email))
                    .Append(Td(string.Join(", ", new[] { user.City, user.Country }.Where(v => !string.IsNullOrWhiteSpace(v)))))
                    .Append(Td(user.WalletBalance.ToString("N0")))
                    .Append(Td(user.DailyTargetSteps?.ToString("N0") ?? "—"))
                    .Append(Td(user.Status))
                    .Append(Td(user.ProfileComplete ? "Yes" : "No"))
                    .Append(Td(user.ProviderUserId))
                    .Append("</tr>");
            }
        }

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>MOVOQ local users</title>
              <style>
                :root { color-scheme: light; }
                body { margin: 0; font-family: Segoe UI, sans-serif; background: linear-gradient(135deg, #eff6ff, #f5f3ff, #fdf2f8); color: #111827; }
                main { max-width: 1100px; margin: 0 auto; padding: 32px 20px 48px; }
                h1 { margin: 0 0 8px; font-size: 28px; }
                p.lead { color: #4b5563; margin: 0 0 24px; }
                .card { background: #fff; border: 1px solid #f3f4f6; border-radius: 20px; padding: 20px; margin-bottom: 20px; box-shadow: 0 8px 24px rgba(0,0,0,.05); }
                .stats { display: flex; gap: 16px; flex-wrap: wrap; }
                .stat { min-width: 140px; }
                .stat b { display: block; font-size: 28px; }
                form.grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; align-items: end; }
                label { display: block; font-size: 12px; color: #6b7280; margin-bottom: 4px; }
                input, select, button { width: 100%; padding: 10px 12px; border-radius: 12px; border: 1px solid #e5e7eb; font: inherit; }
                button { background: #7c3aed; color: #fff; border: 0; font-weight: 700; cursor: pointer; }
                button.secondary { background: #111827; }
                table { width: 100%; border-collapse: collapse; }
                th, td { text-align: left; padding: 10px 8px; border-bottom: 1px solid #f3f4f6; font-size: 14px; }
                th { color: #6b7280; font-size: 12px; text-transform: uppercase; }
                td.empty { text-align: center; color: #6b7280; padding: 28px 8px; }
                .note { font-size: 13px; color: #6b7280; }
              </style>
            </head>
            <body>
              <main>
                <h1>MOVOQ local users</h1>
                <p class="lead">Development only. These fake accounts appear in the database so you can inspect Home and this dashboard.</p>
                <div class="card stats">
                  <div class="stat"><span>Users</span><b>{{users.Count}}</b></div>
                  <div class="stat"><span>Points in wallets</span><b>{{users.Sum(u => u.WalletBalance).ToString("N0")}}</b></div>
                </div>
                <div class="card">
                  <h2>Add a fake user</h2>
                  <form class="grid" method="post" action="/dev/users">
                    <div><label>Name</label><input name="fullName" placeholder="Hina Mir" /></div>
                    <div><label>Email</label><input name="email" placeholder="hina.mir@localhost" /></div>
                    <div><label>City</label><input name="city" placeholder="Lahore" /></div>
                    <div><label>State</label><input name="state" placeholder="Punjab" /></div>
                    <div><label>Country</label><input name="country" value="PK" maxlength="2" /></div>
                    <div><label>Goal</label>
                      <select name="dailyTargetSteps">
                        <option value="">Random</option>
                        <option value="4000">4000</option>
                        <option value="6000">6000</option>
                        <option value="10000">10000</option>
                      </select>
                    </div>
                    <div><label>Points</label><input name="walletBalance" type="number" min="0" placeholder="e.g. 800" /></div>
                    <div><button type="submit">Add user</button></div>
                  </form>
                  <form method="post" action="/dev/seed" style="margin-top:12px;max-width:320px">
                    <button class="secondary" type="submit">Seed 50 users with activity</button>
                  </form>
                  <p class="note" style="margin-top:8px">Creates <code>@localhost</code> walkers, 14 days of step history, and ledger entries for the admin charts.</p>
                  <p class="note">Leave fields blank to generate a random person. Sample users are created only if their emails are missing.</p>
                </div>
                <div class="card">
                  <h2>Directory</h2>
                  <table>
                    <thead>
                      <tr>
                        <th>Name</th><th>Email</th><th>Location</th><th>Points</th><th>Goal</th><th>Status</th><th>Profile</th><th>Local id</th>
                      </tr>
                    </thead>
                    <tbody>{{rows}}</tbody>
                  </table>
                </div>
              </main>
            </body>
            </html>
            """;
    }

    private static string Td(string value) => $"<td>{WebUtility.HtmlEncode(value)}</td>";
}
