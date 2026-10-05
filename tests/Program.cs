using System;
using RepoEmoteWheel;
int passed = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS " + name); passed++; }
for(int i=0;i<6;i++) {
    double a=i*Math.PI/3;
    Check(WheelState.Sector(Math.Sin(a)*140,-Math.Cos(a)*140,55)==i,"sector " + i);
}
Check(WheelState.Sector(0,0,55)==-1,"center cancels");
Check(WheelState.Sector(0,-54.9,55)==-1,"dead zone edge");
Check(WheelState.Sector(0,-55,55)==0,"outside dead zone");
var s = new WheelState();
void Tick(double t,bool held,double x=0,double y=-140,bool allowed=true,bool cancel=false) => s.Tick(t,allowed,held,cancel,x,y,55,5);
Tick(0,true,0,0);Check(s.IsOpen && s.Active==-1,"hold opens without playing");
Tick(1,true);Check(s.Hovered==0 && s.Active==-1,"hover does not fire");
Tick(2,false);Check(!s.IsOpen && s.Active==0,"release fires hovered expression");
Tick(6.999,false);Check(s.Active==0,"expression lasts five seconds");
Tick(7,false);Check(s.Active==-1,"expression expires");
Tick(8,true);Tick(9,false,0,0);Check(s.Active==-1,"release in center cancels");
Tick(10,true);Tick(11,true,cancel:true);Tick(12,true);Check(!s.IsOpen && s.Active==-1,"Escape cancels until released");
Tick(13,false);Tick(14,true);Tick(15,false);Tick(16,false,allowed:false);Check(s.Active==-1,"menu/death/focus loss cancels active emote");
Tick(17,true,allowed:false);Tick(18,true);Check(!s.IsOpen,"held button entering game does not open wheel");
Tick(19,false);Tick(20,true);Tick(21,false);Tick(22,true);Check(s.IsOpen && s.Active==-1,"reopening replaces old expression");
Tick(23,false,140,0);Check(s.Active==2,"release uses current hover, not stale hover");
Tick(24,true,allowed:false);Check(s.Active==-1 && !s.IsOpen,"rebinding cancels playback and selection");
Tick(25,true);Check(!s.IsOpen,"new binding already held requires a fresh press");
Tick(26,false);Check(s.Active==-1,"releasing new binding cannot fire old selection");
Tick(27,true);Tick(28,false);Check(s.Active==0,"fresh press after rebinding plays normally");
var h = new HoldState();
h.Tick(true, true); Check(h.Active, "self-view hold starts");
h.Tick(true, false); Check(!h.Active, "self-view release restores first person");
h.Tick(true, true); h.Tick(false, true); Check(!h.Active, "self-view interruption cancels");
h.Tick(true, true); Check(!h.Active, "self-view stays canceled while key remains held");
h.Tick(true, false); h.Tick(true, true); Check(h.Active, "self-view resumes on fresh press");
h.Cancel(true); h.Tick(true, true); Check(!h.Active, "self-view rebind to held key waits for release");
h.Tick(true, false); h.Tick(true, true); Check(h.Active, "self-view fresh rebound key works");
h.Cancel(false); h.Tick(true, true); Check(h.Active, "self-view rebind to released key works");
h.Tick(false, false); h.Tick(false, true); h.Tick(true, true);
Check(!h.Active, "self-view key pressed in menu cannot activate on menu close");
// Reproduce the native rise/decay loop: held expression weights settle near 50.
foreach (float dt in new[] { 1f / 30, 1f / 60, 1f / 144 }) {
    float neutral = 100, angry = 0;
    for (int frame = 0; frame < 5 / dt; frame++) {
        angry += (100 - angry) * dt * 5;
        neutral += -neutral * dt * 5;
        neutral += -neutral * dt * 5;
        angry += -angry * dt * 5;
    }
    var weights = new[] { neutral, angry, 0f, 0f, 0f, 0f, 0f };
    Check(PoseWeights.Normalize(weights) > .999f && weights[1] > .999f,
        "held native expression reaches full arm pose at " + (int)Math.Round(1 / dt) + " fps");
}
var fade = new[] { 25f, 25f, 0f, 0f, 0f, 0f, 0f };
Check(Math.Abs(PoseWeights.Normalize(fade) - .5f) < .0001f, "neutral weight blends pose out");
var mixed = new[] { 0f, 10f, 30f, 0f, 0f, 0f, 0f };
PoseWeights.Normalize(mixed);
Check(mixed[1] == .25f && mixed[2] == .75f, "simultaneous emotes retain native proportions");
Check(PoseWeights.Normalize(new float[7]) == 0, "empty expressions leave arms alone");
// Real rig sockets: left (0,-.04,.471), right (0,0,.5133).
foreach (float lateral in new[] { .04f, 0f }) {
    float armLength = lateral > 0 ? .471f : .5133f;
    foreach (float targetLength in new[] { .30f, .45f, .60f }) {
        float scale = PoseWeights.ReachScale(lateral * lateral, armLength * armLength, 0, targetLength);
        double actualReach = Math.Sqrt(lateral * lateral + Math.Pow(armLength * scale, 2));
        Check(Math.Abs(actualReach - targetLength) < .00001,
            "palm reaches contact at " + targetLength + " with socket offset " + lateral);
    }
}
Console.WriteLine($"{passed} checks passed.");
