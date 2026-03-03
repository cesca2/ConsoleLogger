using Spectre.Console;

namespace SessionLogger.Models;

// model
internal class Session
{
    internal int Id { get; set; } = 1;
    internal string SessionType { get; set; }
    internal string Date { get; set; }
    internal string StartTime { get; set; }
    internal string EndTime { get; set; }
    internal string Duration {get; set;}

    internal Session(string type, DateTime date, DateTime start, DateTime end) 
    {
       SessionType = type;
       StartTime = start.ToShortTimeString();
       EndTime= end.ToShortTimeString();
       Date = date.ToShortDateString();

       double hours = Math.Floor((end-start).TotalHours);
       double minutes = (end-start).TotalMinutes - Math.Floor((end-start).TotalHours) * 60;

       Duration = $"{hours}h {minutes}m";

    }    

    public void DisplayDetails()
    {
        var panel = new Panel(new Markup(
                                         $"[bold]Type:[/]  [cyan]{SessionType}[/]" + 
                                         $"   [bold]Date:[/]  [blue]{Date}[/]" + 
                                         $"   [bold]Duration:[/]  [green]{Duration}[/]"       
        ))
        {
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }
  
}