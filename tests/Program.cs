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
Console.WriteLine($"{passed} checks passed.");
