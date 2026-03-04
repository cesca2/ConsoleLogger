using Spectre.Console;
using static SessionLogger.Enums;
using SessionLogger.Models;
using SessionLogger.Controllers;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.ComponentModel;
namespace SessionLogger;

internal class UserInterface
{
    private readonly DatabaseController _databaseController = new();

    private void DisplayMessage(string message, string color = "yellow")
        {
            AnsiConsole.MarkupLine($"[{color}]{message}[/]");
        }

    private bool ConfirmAction(string actionName, string color = "yellow")
        {
            var ConfirmAction = AnsiConsole.Confirm($"Are you sure you want to [{color}]{actionName}[/]?");

            return ConfirmAction;
        }
    internal void MainMenu()
    {
        DisplayMessage("");

        AnsiConsole.Write(
            new FigletText("Session logger"));
        DisplayMessage("");
        DisplayMessage("Welcome to the Session Logger application!", "white");
        DisplayMessage("");
        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();

        _databaseController.SQLHandler("""
            CREATE TABLE IF NOT EXISTS sessions (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                type TEXT NOT NULL,
                date TEXT NOT NULL, 
                duration TEXT NOT NULL,
                start TEXT NOT NULL,
                end TEXT NOT NULL
            );

        """);

        while (true)
        {
            Console.Clear();

            var actionChoice = AnsiConsole.Prompt(
                new SelectionPrompt<MenuAction>()
                .Title("Please select an action:")
                .UseConverter(e => System.Text.RegularExpressions.Regex.Replace(e.ToString(), "([a-z])([A-Z])", "$1 $2"))
                .AddChoices(Enum.GetValues<MenuAction>()));

            switch (actionChoice)
            {
                case MenuAction.ViewSessions:
                    ViewSessions();
                    break;
                case MenuAction.AddSession:
                    AddSession();
                    break;
                case MenuAction.DeleteSession:
                    DeleteSession();
                    break;
                case MenuAction.UpdateSession:
                    UpdateSession();
                    break;
            }


        }
    }

    private DateTime? filterSessions()
    {
        var filterChoice = AnsiConsole.Prompt(
            new SelectionPrompt<FilterAction>()
            .Title("Filter date?")
            .UseConverter(e => System.Text.RegularExpressions.Regex.Replace(e.ToString(), "([a-z])([A-Z])", "$1 $2"))
            .AddChoices(Enum.GetValues<FilterAction>()));


        switch (filterChoice)
            {
                case FilterAction.AllTime:
                    return null;
                case FilterAction.LastWeek:
                    return DateTime.Now.AddDays(-7);
                case FilterAction.LastMonth:
                    return DateTime.Now.AddMonths(-1);
                case FilterAction.Custom:                    
                    var filterUnit = AnsiConsole.Prompt(new SelectionPrompt<FilterUnit>()
                    .Title("Filter by number of years, months or days?")
                    .AddChoices(Enum.GetValues<FilterUnit>()));

                    var length= AnsiConsole.Ask<int>($"Enter the number of {filterUnit}:");
                    
                    Console.Clear();
                
                    switch(filterUnit)
                    {
                        case FilterUnit.Days:
                            return DateTime.Now.AddDays(-length);
                        case FilterUnit.Months:
                            return DateTime.Now.AddMonths(-length);
                        case FilterUnit.Years:
                            return DateTime.Now.AddYears(-length);
                        default: return null;
                    
                    }
                    
                default: return null;

            }
        
    }

    private  Session UserInputSession(string message = "add")
    {

        var type= AnsiConsole.Ask<string>($"Enter the [cyan]type[/] of session to {message}:");
        
        var dateprompt = new TextPrompt<string>($"Enter the [blue]date (DD/MM/YYYY)[/] of the session to {message}:")
        .Validate(input =>
            DateTime.TryParse(
                input,
                out _
            ) && DateTime.Parse(input).Date <= DateTime.Now.Date, "[red]Please check date format matches (DD/MM/YYYY), and is not in the future[/]");

        var date= AnsiConsole.Prompt(dateprompt);
    
        var startTime = AnsiConsole.Ask<DateTime>($"Enter the [green]start time (HH:MM) [/] of the session to {message}:");
        var endprompt = new TextPrompt<DateTime>($"Enter the [green]end time (HH:MM)[/] of the session to {message}:")
        .Validate(input => 
                  input > startTime, $"[red]End time must be after specified start time, {startTime.TimeOfDay}[/]");
        
        var endTime = AnsiConsole.Prompt(endprompt);
        
        var newSession= new Session(type, DateTime.Parse(date), startTime, endTime);

        return newSession;
        
    }
    private void ViewSessions()
    {   
        var filterDate = filterSessions();
        
        var table = new Table();
        table.Border(TableBorder.Rounded);

        table.AddColumn("[yellow]Type[/]");
        table.AddColumn("[yellow]Date[/]");
        table.AddColumn("[yellow]Length[/]");
        
        var entries = _databaseController.GetAllRecords(filterDate);

        foreach (var entry in entries)
        {
            table.AddRow(
                $"[cyan]{entry.SessionType}[/]",
                $"[blue]{entry.Date}[/]",
                $"[green]{entry.Duration}[/]"
                );
        }

        AnsiConsole.Write(table);
        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();

       
    }

    private void AddSession()
    {
        var newSession= UserInputSession();
        newSession.DisplayDetails();

        if (ConfirmAction("add this session?"))
        {
            _databaseController.SQLHandler(
            $"""
                INSERT INTO sessions(type, date, duration, start, end) 
                VALUES 
                ('{newSession.SessionType}', 
                '{newSession.Date}', 
                '{newSession.Duration}' , 
                '{newSession.StartTime}', 
                '{newSession.EndTime}')
                ;
            """);
        
            DisplayMessage("Session added");

        }
        else
        {
            DisplayMessage("Addition of session cancelled", "red");
        }

        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();


    }

    private void DeleteSession()
    {
        var filterDate = filterSessions();
        var deletionEntries = _databaseController.GetAllRecords(filterDate);
        var sessionToDelete = AnsiConsole.Prompt(
                new SelectionPrompt<Session>()
                    .Title("Select a [red]session[/] to delete:")
                    .UseConverter(e => $"{e.SessionType}, {e.Date}, {e.Duration} ")
                    .AddChoices(deletionEntries));
        sessionToDelete.DisplayDetails();
        if (ConfirmAction("delete the above entry?", "red"))
        {
            _databaseController.SQLHandler(
            $"""
                DELETE FROM sessions
                WHERE id={sessionToDelete.Id};
            """
            );
            DisplayMessage("Session deleted");
        }
        else
        {
            DisplayMessage("Deletion of session cancelled", "red");
        }
        
        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();


    }

    private void UpdateSession()
    {
        var filterDate = filterSessions();
        var updateEntries = _databaseController.GetAllRecords(filterDate);
        var sessionToUpdate = AnsiConsole.Prompt(
                new SelectionPrompt<Session>()
                    .Title("Select a [yellow]session[/] to update:")
                    .UseConverter(e => $"{e.SessionType}, {e.Date}, {e.Duration} ")
                    .AddChoices(updateEntries));
        
        var newSession= UserInputSession("update");
        
        DisplayMessage("", "white");
        DisplayMessage("Replace");
        sessionToUpdate.DisplayDetails();
        DisplayMessage("with");
        newSession.DisplayDetails();
        
        if (ConfirmAction($"update this entry as detailed above?", "yellow"))
        {
            _databaseController.SQLHandler(
                $"""
                UPDATE sessions
                SET date='{newSession.Date}',
                type='{newSession.SessionType}',
                duration='{newSession.Duration}',
                start='{newSession.StartTime}',
                end='{newSession.EndTime}'
                WHERE id={sessionToUpdate.Id};
                """);

                DisplayMessage("Session updated");
        }
        else
        {
            DisplayMessage("Update of session cancelled", "red");
        }

        DisplayMessage("Press Any Key to Continue.");
        Console.ReadKey();

    }
}
