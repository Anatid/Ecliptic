using System;
using UnityEngine;
using KSP.IO;

namespace Ecliptic
{
    public class EclipticBase : TutorialScenario
    {
        protected static string playerName = "Engineer";
        protected PluginConfiguration settings = PluginConfiguration.CreateForType<EclipticBase>();

        //apparently we can't just use a static initializer because GUI.skin can only be used within OnGUI
        protected static GUIStyle _instrStyle;
        protected static GUIStyle instrStyle
        {
            get
            {
                if (_instrStyle == null) _instrStyle = Style(Color.cyan, FontStyle.Italic);
                return _instrStyle;
            }
        }

        protected void LoadSettings()
        {
            settings.load();
            playerName = settings.GetValue<string>("PLAYER_NAME") ?? "Engineer";
        }

        protected void SaveSettings()
        {
            settings["PLAYER_NAME"] = playerName;
            settings.save();
        }

        //TutorialScenario implements these update functions, but keeps them private.
        //When we reimplement them we hide them from Unity, so we duplicate
        //the TutorialScenario functionality (namely, call Tutorial.XXXXXFSM())
        //and then call OnX, which is virtual so that subclasses can override it properly.
        protected void FixedUpdate()
        {
            if (Tutorial.Started) Tutorial.FixedUpdateFSM();
            OnFixedUpdate();
        }
        protected void Update()
        {
            if (Tutorial.Started) Tutorial.UpdateFSM();
            OnUpdate();
        }
        protected void LateUpdate()
        {
            if (Tutorial.Started) Tutorial.LateUpdateFSM();
            OnLateUpdate();
        }

        protected virtual void OnFixedUpdate() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }




        protected TutorialPage AddPage(String windowTitle, Callback onEnter = null)
        {
            TutorialPage ret = new TutorialPage(windowTitle);
            ret.windowTitle = windowTitle;
            if (onEnter != null) ret.OnEnter += (KFSMState prev) => { onEnter(); };
            Tutorial.AddPage(ret);
            return ret;
        }

        protected TutorialPage AddState(String windowTitle)
        {
            TutorialPage ret = new TutorialPage(windowTitle);
            ret.windowTitle = windowTitle;
            Tutorial.AddState(ret);
            return ret;
        }

        protected KFSMEvent AddEvent(TutorialPage goToPage, string eventName, params KFSMState[] addToStates)
        {
            KFSMEvent e = new KFSMEvent(eventName);
            e.GoToStateOnEvent = goToPage;
            e.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
            Tutorial.AddEvent(e, addToStates);
            return e;
        }

        protected KFSMEvent AddEventExcluding(TutorialPage goToPage, string eventName, params KFSMState[] excludeFromStates)
        {
            KFSMEvent e = new KFSMEvent(eventName);
            e.GoToStateOnEvent = goToPage;
            e.updateMode = KFSMUpdateMode.MANUAL_TRIGGER;
            Tutorial.AddEventExcluding(e, excludeFromStates);
            return e;
        }



        protected void AddConversation(string[] screens, String conversationName, String windowTitle)
        {
            for (int i = 0; i < screens.Length; i++)
            {
                TutorialPage page = AddPage(windowTitle);
                if (i == 0) page.OnEnter += (KFSMState prev) => { UnhideTutorialWindow(); };
                page.OnDrawContent = MakeOnDrawContent(screens[i], i == 0);
            }
        }

        protected Callback MakeOnDrawContent(string screen, bool isFirstPage)
        {
            return () =>
            {
                GUILayout.Label(screen, GUILayout.ExpandHeight(true));
                GUILayout.BeginHorizontal();
                if (!isFirstPage && GUILayout.Button("Back")) Tutorial.GoToLastPage();
                if (GUILayout.Button("Next")) Tutorial.GoToNextPage();
                GUILayout.EndHorizontal();
            };
        }

        protected void HideTutorialWindow()
        {
            SetDialogRect(new Rect(Screen.width, 100, 0, 0));
        }

        protected void UnhideTutorialWindow()
        {
            SetDialogRect(new Rect(Screen.width / 5, Screen.height / 5, 400, 100));
        }

        protected static GUIStyle Style(params object[] args)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            foreach (object arg in args)
            {
                if (arg is Color) s.normal.textColor = (Color)arg;
                else if (arg is TextAnchor) s.alignment = (TextAnchor)arg;
                else if (arg is FontStyle) s.fontStyle = (FontStyle)arg;
                else Debug.Log("EclipticBase::Style got strange argument: type=" + arg.GetType().ToString() + ", value=" + arg.ToString());
            }
            return s;
        }


        protected static String TimeString(double seconds)
        {
            bool minus = (seconds < 0);
            seconds = Math.Abs(seconds);

            int days = (int)(seconds / (3600 * 24));
            seconds -= 3600 * 24 * days;
            int hours = (int)(seconds / 3600);
            seconds -= 3600 * hours;
            int minutes = (int)(seconds / 60);
            seconds -= 60 * minutes;

            String ret = (minus ? "-" : "");
            if (days != 0) ret += days.ToString() + (days > 1 ? " days, " : " day, ");
            if (hours != 0) ret += hours.ToString() + (hours > 1 ? " hours, " : " hour, ");
            if (minutes != 0) ret += minutes.ToString() + (minutes > 1 ? " minutes, " : " minutes, ");
            if ((int)seconds != 0) ret += ((int)seconds).ToString() + ((int)seconds > 1 ? " seconds" : " second");
            return ret;
        }
    }
}
