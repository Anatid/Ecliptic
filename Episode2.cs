using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//Todo:
// 
//
//
// - figure out Eve transfer time
// - replace <TIME> with mission allowed time
//
//  - Write code
//      - fix victory detection false positive on relaunch
//      - explode vessels in the blast zone?
//      - choose the right tutors
//      - look at animations
//      - remove CrewManifestModules


namespace Ecliptic
{
    class Episode2Tracking : EclipticBase
    {
        const double scenarioStartUT = 16763478;

        protected override Instructor npc { get { return Instructor.Werner; } }

        protected override void OnTutorialSetup()
        {
            if (Planetarium.GetUniversalTime() > scenarioStartUT + 5) return;

            Episode2Flight.victorious = false;

            LoadSettings();

            TutorialPage startPage = AddPage("Video Call", UnhideTutorialWindow);
            startPage.OnDrawContent = () =>
            {
                GUILayout.BeginVertical();
                GUILayout.Label("A video call arrives from Kerbal High Command, five minutes after the line went suddently dead. Director Gene Kerman brings it up on the tracking station call screen.", instrStyle);
                if (GUILayout.Button("Next")) Tutorial.GoToNextPage();
                GUILayout.EndVertical();
            };

            AddConversation(new string[]{
                             "Gene Kerman:\nWerner! What happened? The connection just cut out without any warning!",
                             "Werner von Kerman:\nSorry about that. KHC's first instinct in any new crisis is always secrecy. I'm back on the line now that I've overcome the higher-ups' knee jerk reaction. It's an old habit and I'm worried that it's taking far too long to die. It's not like we have any external enemies to worry about anymore... or so we thought.",
                             "Anyway, we're now seeing a distressing number of new tracking targets, all of which appeared suddenly five minutes ago. This time we had simultaneous optical tracking ready to go too. For a few seconds they glowed very bright, then dimmed to mere specks. In radar they also look just like the last one.",
                             "Gene Kerman:\nAh, I think I see...",
                             "Werner von Kerman:\nDo you? Well, I'll spell out our interpretation for you. We think that we're looking at a dozen more missiles of the type Lieutenant " + playerName + " just disarmed. And the initial glow in the visible was rocket engines firing.",
                             "Gene Kerman:\nA course correction.",
                             "Werner von Kerman:\nExactly. The only thing that doesn't explain is why they appeared so suddenly on our radar five minutes ago. We're working on the assumption that they're equipped with some sort of stealth technology--maybe a carefully shaped anti-radar shell?--that they had to discard before performing a burn.", 
                             "But after the first missile was disarmed it sent some sort of signal that caused the others to abandon stealth. Which of course forces us to ask how many *more* of these are already out there. But the more immediate problem is what to do about these.",                             "Gene Kerman:\nYou want " + playerName + " to send up a *dozen* more intercept missions in a matter of a few hours? That's impossible!",
                             "Werner von Kerman:\nI agree. Which is why it's fortunate that these aren't heading for Kerbin. Check the tracking projections. In six hours, these missiles are scheduled to impact the Mun. Our tracking data isn't precise enough to be sure yet, but I'd bet a year's pay that they'll score direct hits on our bases there.",
                             "Gene Kerman:\nOf course they will. Damn it, those are our only manned facilities beyond Kerbin orbit! Our foothold in the cosmos! Where are we if it gets blown up? Who's trying to drive us back to Kerbin? I've spent my whole life preparing kerbals for the great journey outward, and just as we begin someone is sabotaging the entire destiny of the species!",
                             "Werner von Kerman:\nIt was supposed to be worse. KSC itself would have been destroyed if not for Lieutenant " + playerName + "'s mission. We were supposed to be completely deprived of all space capability for quite some time. The new facility in West Aurora won't be online for another two years at least.",
                             "Gene Kerman:\nYes, we've saved KSC, but the munar bases are *not* an acceptable loss! Quite aside from their symbolic importance, there are eight kerbals on the surface of the Mun right now, as the nukes close in. They can't be allowed to die! Can't Kerbal High Command shoot these missiles down or something?!", 
                             "Werner von Kerman:\nNo, we can't. You know we can't. Kerbal High Command has spent the last ten years demilitarizing space. It was about time we stopped holding the sword of annihilation to our own throats. I sleep easier at night knowing there are no more missile platforms orbiting overhead every half an hour.",
                             "Gene Kerman:\nYes, well, that was before we started being attacked by unknown enemies from the skies. An orbital missile platform on our side would be positively relaxing right now. But I suppose it's down to the civilian space program to save the day again.",
                             "Werner von Kerman:\nYes, it's down to you.\n",
                             "Gene Kerman:\nHm. Well, you heard him, " + playerName + "...\n\n(Director Gene Kerman takes a deep breath and focuses on the tracking data.)",
                             "Gene Kerman:\nIt's not going to be easy. We have to assume that we're dealing with nukes, and the munar stations don't have any sort of escape vehicles that would let the crew get far enough away from a nuke to survive.",
                             "Gene Kerman:\nWe'll have to mount an evacuation mission. If we're fast enough, we can get a rescue ship in and out before the missiles drop. It will need to be faster than any previous mission to the Mun, though. A normal transfer from LKO to the Mun would take so long that the missiles will already be impacting as the rescue ship arrives. We'll have to do a more aggressive transfer burn, but that's doable.",
                             "Gene Kerman:\nThe exact mission parameters are up to you, Chief Flight Engineer. I'll defer to you on the design decisions: One ship or several, how to get there fast enough, and how to get our people out. But we know our mission objective: Get the forty-six kerbals on Farside Base to safety. The safe range is probably about 50 km. If you can get every kerbal at least that far away from the nuclear explosions when the missiles go off, we'll have succeeded.",
                             "Gene Kerman:\nAs a secondary objective, you could try to save the hardware. Both Farside Base and the Mun Polar Telescope still have the thrusters that were used to land them, though they're out of fuel. If you could somehow refuel them, you could fly both the people and hardware out of there.",
                             "Gene Kerman:\nWe've got even less time than the last mission: six hours to impact. Good luck."
                           }, "Colonel Werner von Kerman - Video Call", "Colonel Werner von Kerman - Video Call");

            TutorialPage objectivePage = AddPage("Mission Objectives", UnhideTutorialWindow);
            objectivePage.OnDrawContent = () =>
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Primary objective:\n" +
                                " * Evacuate the four kerbals at Farside Base and the four kerbals at the Mun Polar Telescope from the 50 km blast radius around each base.\n\n" +
                                "Secondary objective:\n" +
                                " * Move Farside Base and the Mun Polar Telescope themselves out of the blast radius before the missiles detonate", instrStyle);
                if (GUILayout.Button("Next")) Tutorial.GoToNextPage();
                GUILayout.EndVertical();
            };

            TutorialPage finishedBriefingPage = AddPage("Finished Briefing", HideTutorialWindow);

            Tutorial.StartTutorial(startPage);
        }
    }

    class Episode2Flight : EclipticBase
    {
        double victoryUT;
        const double scenarioStartUT = 16763478;
        public static bool victorious = false;

        protected override Instructor npc { get { return Instructor.Werner; } }

        void Init()
        {
            LoadSettings();
            RenderingManager.AddToPostDrawQueue(0, DrawStatus);
            GameEvents.onKerbalStatusChange.Add(OnKerbalStatusChange);
        }

        public void OnKerbalStatusChange(ProtoCrewMember kerbal, ProtoCrewMember.RosterStatus oldStatus, ProtoCrewMember.RosterStatus newStatus)
        {
            if (newStatus == ProtoCrewMember.RosterStatus.Dead)
            {
                Tutorial.RunEvent(onKerbalDeath);
            }
        }

        KFSMEvent onKerbalDeath;
        protected override void OnTutorialSetup()
        {
            Init();

            TutorialPage waitForVictoryPage, toBeContinuedPage;
            if (!victorious)
            {

                waitForVictoryPage = AddPage("Wait for victory", HideTutorialWindow);
                waitForVictoryPage.SetAdvanceCondition(state => (NumKerbalsInBlastZone() == 0));
                waitForVictoryPage.OnLeave = (state => { victoryUT = Planetarium.GetUniversalTime(); });

                AddConversation(new string[]{
                "Gene Kerman:\nNicely done! After watching that, " + playerName + ", I'll take you over an orbital missile platform any day of the week.",
                "Gene Kerman:\nLet me know when you're finished wrapping up the mission. I've got some more information you'll be very interested in.",
                "Gene Kerman:\nOK, let me get Werner on the line first.\n\n(The Director fiddles with the video screen.)",
                "Werner von Kerman:\nI'm here, Gene. Now what's this news you're being so coy about?",
                "Gene Kerman:\nI'd prefer \"cautious\" to \"coy,\" Werner. We like to make sure of these things. It's quite a befuddling result. Luckily we had the prediction programs for this already working, since we were already beginning to plan this sort of mission. Just ran them backwards, more or less--",
                "Werner von Kerman:\nOh, just spit it out!",
                "Gene Kerman:\nWell, to make a long story short, while " + playerName + " was rescuing our munar colonists, I had the scientists at KSC use your tracking data to figure out what the missiles' orbits were before their correction burns, and then we projected those orbits backward in time.",
                "Werner von Kerman:\n--And?!",
                "Gene Kerman:\nAnd, well, and we're pretty sure of this, the projections say the missiles left low Eve orbit on a transfer trajectory for Kerbin one hundred and fourty-four days ago.",
                "Werner von Kerman:\n\n...",
                "Werner von Kerman:\n\n... ...",
                "Werner von Kerman:\n\n... ... ...WHAT?! But nobody's ever BEEN to Eve!"},
                    "Director Gene Kerman - Video Call", "Director Gene Kerman - Video Call");

                toBeContinuedPage = AddPage("Video Call - Gene Kerman");
                toBeContinuedPage.OnDrawContent = () =>
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("TO BE CONTINUED", Style(Color.yellow, TextAnchor.UpperCenter, FontStyle.Bold));
                    if (GUILayout.Button("End Mission")) Tutorial.GoToNextPage();
                    GUILayout.EndVertical();
                };
            }

            TutorialPage statsPage = AddPage("Mission Stats", UnhideTutorialWindow);
            statsPage.OnEnter += (KFSMState state) => { victorious = true; };
            statsPage.OnDrawContent = () =>
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Time to mission success: " + TimeString(victoryUT - scenarioStartUT), Style(Color.yellow));
                GUILayout.Label("Mission hardware left in blast zone: $" + EquipmentCostInBlastZone().ToString("#,##0"), Style(Color.yellow));
                GUILayout.EndVertical();
            };

            TutorialPage failurePageMissiles = AddPage("Mission Failed", UnhideTutorialWindow);
            failurePageMissiles.OnDrawContent = () =>
            {
                GUILayout.Label("As the missiles' trajectories intersect the munar surface, two clusters of blinding points of light illuminate the Mun's far side and south pole. When the static clears, desperate radio calls fail to raise any of the kerbals in the blast zones.", instrStyle);

                GUILayout.Label("MISSION FAILURE", Style(TextAnchor.UpperCenter, Color.red, FontStyle.Bold));

                GUILayout.Label("You failed to evacuate all kerbals from the blast zone.", Style(Color.red));
            };
            Tutorial.AddState(failurePageMissiles);

            TutorialPage failurePageDeath = AddPage("Mission Failed", UnhideTutorialWindow);
            failurePageDeath.OnDrawContent = () =>
            {
                GUILayout.Label("MISSION FAILURE", Style(TextAnchor.UpperCenter, Color.red, FontStyle.Bold));

                GUILayout.Label("Your rescue mission failed: a kerbal died.", Style(Color.red));
            };
            Tutorial.AddState(failurePageDeath);



            KFSMEvent onFailure = new KFSMEvent("On Failure");
            onFailure.GoToStateOnEvent = failurePageMissiles;
            onFailure.updateMode = KFSMUpdateMode.FIXEDUPDATE;
            onFailure.OnCheckCondition = (KFSMState state) => (!victorious && TimeToImpact() < 0);
            Tutorial.AddEventExcluding(onFailure, failurePageMissiles, failurePageDeath);

            onKerbalDeath = AddEventExcluding(failurePageDeath, "On Failure 2", failurePageMissiles, failurePageDeath);

            Tutorial.StartTutorial(Tutorial.pages[0]);
        }

        static Rect statusWindowPos = new Rect(0, 0, 100, 100);
        void DrawStatus()
        {
            statusWindowPos = GUILayout.Window(8720987, statusWindowPos, DrawStatusWindow, "Mission Status", GUILayout.Width(minimized ? 100.0F : 200.0F), GUILayout.Height(35.0F));
        }

        bool minimized = false;
        void DrawStatusWindow(int windowID)
        {
            if (minimized)
            {
                minimized = !GUILayout.Button("Maximize");
            }
            else
            {
                GUILayout.BeginVertical();

                Color c;

                c = (TimeToImpact() < 3600 ? Color.red : Color.yellow);
                GUILayout.Label("Time to impact: " + TimeString(TimeToImpact()), Style(c));

                double distance = ImpactPositions().Min(x => Vector3d.Distance(x, FlightGlobals.ActiveVessel.transform.position)) / 1000;
                c = (InBlastZone(FlightGlobals.ActiveVessel) ? Color.red : Color.green);
                GUILayout.Label("Distance from nearest impact site: " + distance.ToString("F1") + " km", Style(c));

                c = (NumKerbalsInBlastZone() == 0 ? Color.green : Color.red);
                GUILayout.Label("Kerbals in blast zone: " + NumKerbalsInBlastZone(), Style(c));

                c = (EquipmentCostInBlastZone() == 0 ? Color.green : Color.yellow);
                GUILayout.Label("Equipment in blast zone: $" + EquipmentCostInBlastZone().ToString("#,##0"), Style(c));

                minimized = GUILayout.Button("Minimize");

                GUILayout.EndVertical();
            }

            GUI.DragWindow();
        }


        double TimeToImpact()
        {
            double impactTime = scenarioStartUT + 6*3600;
            return impactTime - Planetarium.GetUniversalTime();
        }

        double EquipmentCostInBlastZone()
        {
            return 1000.0 * FlightGlobals.Vessels.Where(v => InBlastZone(v)).Sum(v => VesselEquipmentCost(v));
        }

        float VesselEquipmentCost(Vessel v)
        {
            return v.protoVessel.protoPartSnapshots.Sum(snapshot => PartLoader.getPartInfoByName(snapshot.partName).cost);
        }

        int NumKerbalsInBlastZone()
        {
            return FlightGlobals.Vessels.Where(v => InBlastZone(v)).Sum(v => VesselCrewCount(v));
        }

        int VesselCrewCount(Vessel v)
        {
            return (v.loaded ? v.GetCrewCount() : v.protoVessel.protoPartSnapshots.Sum(pps => pps.protoModuleCrew.Count));
        }
        
        bool InBlastZone(Vessel v)
        {
            return ImpactPositions().Any(x => Vector3d.Distance(x, v.transform.position) < 50000);
        }

        List<Vector3d> ImpactPositions()
        {
            int N = 2;
            double[] latitudes = { 7.5, -85.65 };
            double[] longitudes = { -150.5, -39.5 };
            double[] altitudes = { 1619, 0 };
            CelestialBody mun = FlightGlobals.Bodies[2];

            List<Vector3d> ret = new List<Vector3d>();
            for (int i = 0; i < N; i++)
            {
                ret.Add(mun.position + (mun.Radius + altitudes[i]) * mun.GetSurfaceNVector(latitudes[i], longitudes[i]));
            }
            return ret;
        }
    }
}
