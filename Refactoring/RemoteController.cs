using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Refactoring
{
    public abstract class Function
    {
        public abstract Predicate<string[]> ActivationCondition { get; }
        public abstract Func<string[], string> Action { get; }
        protected readonly RemoteController Rc;

        protected Function(RemoteController rc)
        {
            Rc = rc;
        }
    }
    
    public class TvOnOff : Function
    {
        public TvOnOff(RemoteController rc) : base(rc) { }
        public readonly IReadOnlyDictionary<string, bool> Options = new Dictionary<string, bool>
        {
            ["on"] = true,
            ["off"] = false
        };

        public override Predicate<string[]> ActivationCondition =>
            args => args[0] == "tv" && Options.ContainsKey(args[1]);
        public override Func<string[], string> Action =>
            args =>
            {
                Rc.IsOnline = Options[args[1]];
                return "";
            };
    }
    
    public class ShowOptions : Function
    {
        public ShowOptions(RemoteController rc) : base(rc) { }
        
        public override Predicate<string[]> ActivationCondition => 
            args => args[0] == "options" && args[1] == "show";
        public override Func<string[], string> Action => 
            args => Rc.OptionsShow();
    }
    
    public class ChangeSettings : Function
    {
        public ChangeSettings(RemoteController rc) : base(rc) { }
        public readonly IReadOnlyDictionary<string, int> Options = new Dictionary<string, int>
        {
            ["up"] = 10,
            ["down"] = -10
        };

        public override Predicate<string[]> ActivationCondition =>
            args => Rc.Settings.ContainsKey(args[0]) && Options.ContainsKey(args[1]);
        public override Func<string[], string> Action =>
            args =>
            {
                Rc.Settings[args[0]] += Options[args[1]];
                return "";
            };
    }

    public class RemoteController
    {
        public readonly Dictionary<string, int> Settings = new Dictionary<string, int>
        {
            ["volume"] = 20,
            ["brightness"] = 20,
            ["contrast"] = 20
        };

        public readonly IReadOnlyList<Function> Functions;
        public bool IsOnline;

        public RemoteController()
        {
            Functions = new List<Function>
            {
                new TvOnOff(this), new ShowOptions(this), new ChangeSettings(this)
            };
        }

        public string Call(string command)
        {
            var args = Regex.Replace(command, "options change ", "", RegexOptions.IgnoreCase).ToLower().Split(' ');
            
            foreach (var function in Functions)
                if (function.ActivationCondition(args))
                    return function.Action.Invoke(args);
            
            throw new ArgumentException($"{command} is not a valid command");
        }

        public string OptionsShow()
        {
            return $@"Options:
Volume {Settings["volume"]}
IsOnline {IsOnline}
Brightness {Settings["brightness"]}
Contrast {Settings["contrast"]}";
        }
    }
}    