using System;
using System.Linq;
using UnityEngine;

//  Beta testers: Supernovy, nicolas, crw70

namespace Ecliptic
{
    public class Episode1Tracking : EclipticBase
    {
        TutorialPage firstBriefingPage, finishedBriefingPage;
        public static bool finishedBriefing = false;

        protected override void OnTutorialSetup()
        {
            if (finishedBriefing) return;

            LoadSettings();

            firstBriefingPage = AddPage("At the Tracking Station");
            firstBriefingPage.OnDrawContent = () =>
            {
                GUILayout.BeginVertical();
                GUILayout.Label("You arrive at the tracking station to find Director Gene Kerman in a video call with someone from Kerbal High Command.", instrStyle, GUILayout.ExpandHeight(true));
                if (GUILayout.Button("Next")) Tutorial.GoToNextPage();
                GUILayout.EndVertical();
            };

            string[] briefingConversation = { 
                                 "This him? Right, Gene, let me talk to him.",
                                 "You there. " + playerName + ". I told the Director to get me KSC's Chief Flight Engineer. That you?",
                                 "I'm Colonel Werner von Kerman at Kerbal High Command. We've got a problem. As of this moment I'm drafting you to be KHC's top representative at KSC in an urgent and secret matter.",
                                 "Two hours ago KHC's satellite tracking radar detected an object entering the Kerbin system from interplanetary space. It took us an hour to get enough data for an approximate orbit solution. When we did, we discovered that the object is headed straight for Kerbin.",
                                 "Now I know what you're thinking: an asteroid! That's what we thought too. This thing is bright in radar, and for ten terrible minutes we thought this was a huge, killer impactor that we had somehow missed until this moment. We thought the kerbal species had hours left to live.",
                                 "Then we got optical telescopes pointed at it. Turns out it's a speck, too small to resolve. It's much too small to be an asteroid. But we've got no idea what it is. In radar it looks for all the world like a kerbal-made rocket. But it can't be: we know the orbit of every single piece of debris put into space in the last century, and this object doesn't match any of those orbits.",
                                 "So here's where you come in. We need a manned mission to rendezvous with this object and see what it is, and we need it launched yesterday. Gene says you're amazing at throwing together a rocket for any task, and you'd better be. I'm making you a lieutenant in the Kerbal High Command for the duration.",
                                 "The object hits atmosphere in twenty-five hours, by which time it will be too late to do anything about it. Your mission is to send someone up there to check it out. When we've had a look at it up close we can decide what to do about it. Make sure it's a crewed mission. We don't know what we'll find up there, so we'll need a kerbal on the spot.",
                                 "But do it quiet-like! The higher-ups at KHC want all this top-secret. No need to cause a panic, eh?",
                                 "We'll be in radio contact with your crew when they get within close range of the object. Remember, twenty-five hours to impact, and we have no idea what that means until we get eyes on this object. Have a look at the tracking data on your console and then get to work, Lieutenant " + playerName + "."
                             };

            AddConversation(briefingConversation, "Tracking Station Briefing", "Colonel Werner von Kerman - Video Call");

            finishedBriefingPage = AddPage("Finished Briefing State", HideTutorialWindow);
            finishedBriefingPage.OnEnter += (KFSMState prev) =>
            {
                finishedBriefing = true;
            };



            Tutorial.StartTutorial(Tutorial.pages[0]);
        }

    }

    public class Episode1Flight : EclipticBase
    {

        const double scenarioStartUT = 16675352.5125756;

        const double titleScreenStartUT = scenarioStartUT + 5;
        const double titleScreenEndUT = titleScreenStartUT + 83;
        bool endedTitleScreen = false;

        public static Episode1Flight instance;

        double victoryUT;

        protected override void OnAssetSetup()
        {
            instructorPrefabName = "Instructor_Gene";
        }


        //        public override void OnAwake()
        void Init()
        {
            base.OnAwake();

            instance = this;

            LoadSettings();

            //reset Episode1Tracking.finishedBriefing at the beginning of the scenario:
            if (Planetarium.GetUniversalTime() < scenarioStartUT + 5)
            {
                Episode1Tracking.finishedBriefing = false;

                RenderingManager.AddToPostDrawQueue(0, new Callback(DrawTitleScreen));
            }
            else
            {
                Episode1Tracking.finishedBriefing = true;
            }
        }


        protected override void OnFixedUpdate()
        {
            if (!Episode1Tracking.finishedBriefing)
            {
                if (!endedTitleScreen && Planetarium.GetUniversalTime() > titleScreenEndUT)
                {
                    RenderingManager.RemoveFromPostDrawQueue(0, DrawTitleScreen);
                    UnhideTutorialWindow();
                    endedTitleScreen = true;
                }
            }

            Vessel incoming = IncomingObjectVessel();
            if (incoming != null)
            {
                double altitudeInFiveTimeSteps = incoming.mainBody.GetAltitude(incoming.transform.position) - 5 * TimeWarp.fixedDeltaTime * incoming.orbit.GetVel().magnitude;
                if (altitudeInFiveTimeSteps < 70000  //object has hit atmosphere
                    || (incoming.orbit.meanAnomaly > 0 && incoming.orbit.PeA < 70000)   //object has warped through planet, probably
                    || !incoming.orbit.referenceBody.bodyName.ToLower().Contains("kerbin")) //object has warped through planet, probably
                {
                    Tutorial.RunEvent(onMissionFailure);
                }

                if (incoming.rootPart != null && !incoming.rootPart.Modules.Contains("ModuleDisarm"))
                {
                    incoming.rootPart.AddModule("ModuleDisarm");
                }
            }
        }



        protected override void OnTutorialSetup()
        {
            Init();

            //only show the title screen at the very start of the scenario:
            if (Planetarium.GetUniversalTime() < scenarioStartUT + 5)
            {
                OnTutorialSetupBeforeBriefing();
            }
            else
            {
                OnTutorialSetupAfterBriefing();
            }
        }

        TutorialPage namePage, callPage, finishedPage;
        protected void OnTutorialSetupBeforeBriefing()
        {
            namePage = AddPage("At the desk of the Chief Flight Engineer", HideTutorialWindow);
            namePage.OnDrawContent = () =>
            {
                if (!endedTitleScreen) return;
                GUILayout.Label("At your desk at the Kerbal Space Center, the phone rings. It's a call from Gene Kerman, the Director of Kerbal Space Center. Before you answer it, what is your name?", instrStyle);
                playerName = GUILayout.TextField(playerName);
                if (GUILayout.Button("Enter"))
                {
                    SaveSettings();
                    Tutorial.GoToNextPage();
                }
            };

            callPage = AddPage("Director Gene Kerman - Video Call");
            callPage.OnDrawContent = () =>
            {
                GUILayout.Label("Hi there, " + playerName + ". I need you to report to the tracking station immediately. We've got an urgent call on the line from an old friend of mine at KHC. I'll meet you there.\n\n");
                GUILayout.Label("End this flight and go to the Tracking Station", instrStyle);
                if (GUILayout.Button("\"Right away, Director\"")) Tutorial.GoToNextPage();
            };

            finishedPage = AddPage("Finished Page", HideTutorialWindow);


            Tutorial.StartTutorial(namePage);
        }


        TutorialPage initialPage, waitForEVAPage, waitForDisarmPage, lastCallPage, statsPage, failurePage;
        KFSMEvent onMissionFailure, onMissionSuccess;
        protected void OnTutorialSetupAfterBriefing()
        {
            initialPage = AddPage("Initial State", HideTutorialWindow);
            initialPage.SetAdvanceCondition((KFSMState state) =>
            {
                HideTutorialWindow(); //because once the on-enter one didn't seem to work
                Vessel incoming = IncomingObjectVessel();
                if (incoming == null) return false;
                double distance = Vector3d.Distance(IncomingObjectVessel().transform.position, FlightGlobals.ActiveVessel.transform.position);
                return (distance < 1000);
            });

            AddConversation(new string[] { "Good, you've made it to the object. Let's get a really close look. Can you EVA and fly over to the object? Have your crewkerbal grab onto the object when they get there." },
                "Orbit Conversation 1", "Director Gene Kerman - Video Call");

            waitForEVAPage = AddPage("Wait For EVA Page", HideTutorialWindow);
            waitForEVAPage.SetAdvanceCondition((KFSMState state) =>
            {
                Vessel incoming = IncomingObjectVessel();
                if (incoming == null) return false;                                            //make sure incoming object exists
                Vessel active = FlightGlobals.ActiveVessel;
                if (active.vesselType != VesselType.EVA) return false;                              //check if player is on an EVA
                if (!active.rootPart.Modules.OfType<KerbalEVA>().First().OnALadder) return false;   //eheck if player is on a ladder

                //check if incoming object is closest vessel
                double incomingObjectDistance = Vector3d.Distance(active.transform.position, incoming.transform.position);
                if (FlightGlobals.Vessels.Any(
                    v => (v != incoming && v != active && Vector3d.Distance(v.transform.position, active.transform.position) < incomingObjectDistance))) return false;

                return true;
            });

            AddConversation(new string[] {
                playerName + ", I've got Werner on the line with me, and we're looking at a feed from a helmet camera. As you've all realized, it's a discarded rocket stage... But that simply can't be! You said it yourself, Werner, we know the location of every scrap of metal that's ever left Kerbin's atmosphere.",
                "Wait, what's that in the camera feed? Those electronics bolted on next to the engine? Those are not standard circuits; I certainly don't recognize them. And they're attached to a slab of something else I don't recognize.",
                "(Relayed through the KSC Director's connection, you hear the accented voice of Colonel Werner von Kerman:)\n“That, my friend, is a solid block of high explosive.”",
                "What?! -- But then--",
                "Colonel Werner von Kerman:\n“Yes.  If those explosives go off, the nuclear engine they're strapped to goes supercritical, and everyone nearby has a bad day. Hm. Gene, does your team have a better orbit solution now that you have close-range radar from Lieutenant " + playerName + "'s mission?”",
                "We should, let me pull it up... Yes, we've got a very precise orbit now. In fact this should be good enough to give a confident prediction of the impact location... Oh. How improbable. If we don't do anything, it's predicted to hit the ground within a kilometer of Kerbal Space Center.",
                "Colonel Werner von Kerman:\n“I don't know if 'improbable' is the right word, Gene. If I were the one who launched this rocket, this is exactly the course I'd put it on if I wanted to cripple the planet's space capability. But what's the motivation for such a crime? Maybe I would know if I understood how this rocket came to be here in the first place.”",
                "While the Colonel ponders that, " + playerName + ", disarm those explosives!"},
                "Orbit Conversation 2", "Director Gene Kerman - Video Call");

            waitForDisarmPage = AddPage("Wait for disarm");
            waitForDisarmPage.OnDrawContent = () =>
            {
                GUILayout.Label("Right click the nuclear engine to disarm the explosives that have been planted on it.", instrStyle);
            };

            lastCallPage = AddPage("Director Gene Kerman - Video Call");
            lastCallPage.OnEnter += (KFSMState prev) => { victoryUT = Planetarium.GetUniversalTime(); };
            lastCallPage.OnDrawContent = () =>
            {
                GUILayout.BeginVertical();
                GUILayout.Label("You've done it! What a relief, even if we still don't know what the hell was going on--");
                GUILayout.Label("In the background, you hear shouting at the KHC end of the connection:", instrStyle);
                GUILayout.Label("“Colonel, radar contact! Correction, contacts! Multiple radar contacts beyond minimal orbit! They all appeared at once! More--I don't know how many! I think they're–”");

                GUILayout.Label("TO BE CONTINUED", Style(TextAnchor.UpperCenter, Color.yellow, FontStyle.Bold), GUILayout.ExpandHeight(true));

                if (GUILayout.Button("End Mission")) Tutorial.GoToNextPage();
                GUILayout.EndVertical();
            };

            //Stats screen
            statsPage = AddPage("Mission Stats");
            statsPage.OnDrawContent = () =>
            {
                GUILayout.Label(String.Format("Time to mission success: " + TimeString(victoryUT - scenarioStartUT)), Style(Color.yellow));
            };

            //Failure page
            failurePage = AddPage("Mission Failed", UnhideTutorialWindow);
            failurePage.OnDrawContent = () =>
            {
                GUILayout.Label("The incoming object enters the atmosphere. Thirty seconds later, Kerbal Space Center disappears into the glow of a blinding fireball, and a billowing black cloud ascends over the destruction.", instrStyle);

                GUILayout.Label("MISSION FAILURE", Style(TextAnchor.UpperCenter, Color.red, FontStyle.Bold));

                GUILayout.Label("You allowed the incoming object to strike Kerbin.", Style(Color.red));
            };


            onMissionFailure = AddEventExcluding(failurePage, "On Mission Failure", failurePage, lastCallPage, statsPage);

            onMissionSuccess = AddEventExcluding(lastCallPage, "On Mission Success", failurePage, lastCallPage, statsPage);

            Tutorial.StartTutorial(initialPage);
        }



        protected Vessel IncomingObjectVessel()
        {
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if (v.vesselName == "Incoming Object") return v;
            }
            return null;
        }

        public void OnDisarm()
        {
            Tutorial.RunEvent(onMissionSuccess);
        }


        void DrawTitleScreen()
        {
            FlightCamera.fetch.mode = FlightCamera.Modes.FREE;
            FlightCamera.CamHdg = Mathf.PI / 2;
            FlightCamera.CamPitch = Mathf.PI / 8;

            string scrollText =
                "The date is Minday, Pan 23rd, 2940.\n\n" +
                "In the hundred years since kerbals first set foot\n" +
                "on the Mun, much has changed. At first, those daring\n" +
                "steps faded from memory. The people of Kerbin\n" +
                "turned back from the sky, and resumed their\n" +
                "petty conflicts. But now that Kerbin is politically\n" +
                "unified and the militaries of all nations have been\n" +
                "subsumed under the Kerbal High Command, war is a \n" +
                "thing of the past, and people are again looking\n" +
                "out toward the solar system, and beyond.\n\n" +
                "A bright new era is dawning.\n";

            float time = (float)(Planetarium.GetUniversalTime() - titleScreenStartUT);
            if (time < 0) time = 0;

            GUIStyle centered = new GUIStyle();
            centered.alignment = TextAnchor.UpperCenter;
            centered.normal.textColor = Color.yellow;
            centered.fontSize = 30;
            GUI.Label(new Rect(Screen.width / 2 - 400, 500 - 15 * time, 800, 15 * time), scrollText, centered);


            float fadeIn1 = 52;
            float fadeIn2 = fadeIn1 + 2;
            float fadeIn3 = fadeIn2 + 5;
            float fadeIn4 = fadeIn3 + 2;

            GUIStyle fadeStyle = new GUIStyle();
            fadeStyle.alignment = TextAnchor.UpperCenter;
            fadeStyle.fontSize = 40;
            float alpha;
            if (time < fadeIn1) alpha = 0;
            else if (time < fadeIn2) alpha = (time - fadeIn1) / (fadeIn2 - fadeIn1);
            else if (time < fadeIn3) alpha = 1;
            else if (time < fadeIn4) alpha = (fadeIn4 - time) / (fadeIn4 - fadeIn3);
            else alpha = 0;
            fadeStyle.normal.textColor = new Color(0F, 0F, 1F, alpha);
            GUI.Label(new Rect(Screen.width / 2 - 400, 200, 800, 500), "Anatid Productions Presents", fadeStyle);


            float fadeIn5 = fadeIn4 + 5;
            float fadeIn6 = fadeIn5 + 3;
            float fadeIn7 = fadeIn6 + 7;
            float fadeIn8 = fadeIn7 + 5;
            fadeStyle.fontSize = 180;
            if (time < fadeIn5) alpha = 0;
            else if (time < fadeIn6) alpha = (time - fadeIn5) / (fadeIn6 - fadeIn5);
            else if (time < fadeIn7) alpha = 1;
            else if (time < fadeIn8) alpha = (fadeIn8 - time) / (fadeIn8 - fadeIn7);
            else alpha = 0;
            fadeStyle.normal.textColor = new Color(0F, 0F, 1F, alpha);
            GUI.Label(new Rect(Screen.width / 2 - 400, 200, 800, 500), "ECLIPTIC", fadeStyle);

            float fadeIn9 = fadeIn6 + 2;
            float fadeIn10 = fadeIn9 + 2;
            fadeStyle.fontSize = 35;
            if (time < fadeIn9) alpha = 0;
            else if (time < fadeIn10) alpha = (time - fadeIn9) / (fadeIn10 - fadeIn9);
            else if (time < fadeIn7) alpha = 1;
            else if (time < fadeIn8) alpha = (fadeIn8 - time) / (fadeIn8 - fadeIn7);
            else alpha = 0;
            fadeStyle.normal.textColor = new Color(0F, 0F, 1F, alpha);
            GUI.Label(new Rect(Screen.width / 2 - 400, 430, 800, 500), "Episode One", fadeStyle);
        }

    }



    public class ModuleDisarm : PartModule
    {
        [KSPEvent(guiActive = true, guiName = "Disarm Explosives", guiActiveUnfocused = true, unfocusedRange = 5)]
        public void OnDisarm()
        {
            Episode1Flight.instance.OnDisarm();
        }
    }

}
