using coolbeats.scripts.logicScripts.Bases;
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

public partial class LifetimeManagement : managerNode
{
	public Godot.Timer timer = new Godot.Timer();
	public double timeFlag = 0;
	public SortedDictionary<double, List<timedObject>> queuedObjects = new SortedDictionary<double, List<timedObject>>();
	bool removeFlag = false;
	readonly object _lock = new object();
	double removeLock = -1;
	double timeoutLock = -2;
	public override void setup()
	{
		timer.Timeout += onTimeout;
		timer.OneShot = true;
		AddChild(timer);
	}
	public void queueTimer(timedObject node, double time)
	{
		double newTime = time + timeFlag + timer.WaitTime - timer.TimeLeft;
		if (!queuedObjects.ContainsKey(newTime))
		{
			queuedObjects[newTime] = new List<timedObject>();
		}
		queuedObjects[newTime].Add(node);
		if (newTime == queuedObjects.First().Key) {
			timeFlag += timer.WaitTime - timer.TimeLeft;
			timer.Start(time);
		}
	}
	public void Remove(timedObject node)
	{
		double key = queuedObjects.First(x => x.Value.Contains(node)).Key;
		removeLock = key;
		if (removeLock == timeoutLock)
		{
			lock (_lock)
			{
				tryRemove(key, node);
			}
		}
		queuedObjects[key].Remove(node);
		removeLock = -1;
	}
	public void tryRemove(double key, timedObject node)
	{
		if (queuedObjects.ContainsKey(key))
		{
			queuedObjects[key].Remove(node);
		}
	}
	public void onTimeout()
	{
		double key = queuedObjects.First().Key;
		timeoutLock = key;
		if (removeLock == timeoutLock)
		{
			lock (_lock)
			{
				foreach (timedObject t in queuedObjects[key])
				{
					t.timeout();	
				} 
			}
		}

		timeFlag += timer.WaitTime;
		queuedObjects.Remove(queuedObjects.First().Key);
		if (queuedObjects.Any())
		{
			timer.Start(queuedObjects.First().Key - timeFlag);
		}
		timeoutLock = -2;
	}
}
