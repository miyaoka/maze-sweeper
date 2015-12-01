using UnityEngine;
using System.Collections;
using UniRx;

public class EdgeType {

	public ReactiveProperty<bool> isPassable = new ReactiveProperty<bool> ();
	public ReactiveProperty<bool> isBreachable = new ReactiveProperty<bool> ();
	public ReactiveProperty<bool> isBreached = new ReactiveProperty<bool> ();
	public ReactiveProperty<int> doorCount = new ReactiveProperty<int> ();

	public EdgeType(bool passable, bool breachable, int doors, bool breached = false){
		isPassable.Value = passable;
		isBreachable.Value = breachable;
		doorCount.Value = doors;
		isBreached.Value = breached;
	}
	public static EdgeType unbreachableWall(){
		return new EdgeType (false, false, 0);
	}
	public static EdgeType wall(){
		return new EdgeType (false, true, 0);
	}
	public static EdgeType passage(){
		return new EdgeType (true, false, 0);
	}
	public static EdgeType door(int doors){
		return new EdgeType (false, true, doors);
	}
	public void breach(){
		if (!isBreachable.Value) {
			return;
		}
		doorCount.Value = 0;
		isPassable.Value = true;
		isBreached.Value = true;
	}
}
