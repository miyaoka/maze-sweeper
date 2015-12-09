using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;
public class EdgePresenter : MonoBehaviour{
	[SerializeField] Image edgeImage;
	[SerializeField] GameObject view;


	CompositeDisposable typeResources = new CompositeDisposable();
	CompositeDisposable modelResources = new CompositeDisposable();


	private EdgeModel model;
	public EdgeModel Model
	{
		set { 
			this.model = value; 
	
			modelResources.Clear ();

			//change image by edgetype
			model.type
				.Where(t => t != null)
				.Subscribe(t => {
					typeResources.Clear();
					t.isPassable
						.Subscribe(p => edgeImage.gameObject.SetActive(p))
						.AddTo(typeResources);
				})
				.AddTo(this);	


			//player is on the one of nodes
			model.nodes[0].onHere
				.CombineLatest(model.nodes[1].onHere, (l,r) => l | r)
				.Subscribe(b => {
					edgeImage.color = b ? new Color(.8f, .8f, .8f) : new Color(.4f, .4f, .4f);
				})
				.AddTo(this);

			//visited one of nodes
			model.nodes[0].visited
				.CombineLatest(model.nodes[1].visited, (l,r) => l | r)
				.Subscribe(b => {
					view.SetActive(b);
				})
				.AddTo(this);

		}
		get { return this.model; }
	}
	/*
	void Start () {
//		view.SetActive (false);

		//両端nodeが設定されている場合
		Observable
			.CombineLatest<NodePresenter> (nodeFrom, nodeTo)
			.Subscribe (nlist => {
				nodeResources.Clear ();
				if(nlist.Contains(null)){
					view.SetActive(false);
					return;
				}
				//接nodeに居る場合はハイライト
				nodeFrom.Value.onHere
					.CombineLatest(nodeTo.Value.onHere, (l,r) => l || r)
					.Subscribe(h => {
						edgeImage.color = h ? new Color(.8f, .8f, .8f) : new Color(.4f, .4f, .4f);
					})
					.AddTo(nodeResources);
				//接node未探訪の場合は非表示
				nodeFrom.Value.visited
					.CombineLatest(nodeTo.Value.visited, (l,r) => l || r)
					.Subscribe(v => {
						view.SetActive(v);
					})
					.AddTo(nodeResources);
		}).AddTo (this);
		
		type
			.Where(t => t != null)
			.Subscribe(t => {
				typeResources.Clear();
				t.isPassable
					.Subscribe(p => edgeImage.gameObject.SetActive(p))
					.AddTo(typeResources);
			})
			.AddTo(this);	
	}
	public void breach(){
		var t = type.Value;
		t.breach ();
		type.Value = type.Value.breach ();
	}
		*/
	void OnDestroy()
	{
		typeResources.Dispose ();
	}
}
