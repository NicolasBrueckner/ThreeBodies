#region

using UnityEngine;
using UnityEngine.UIElements;

#endregion

public class MainUI : MonoBehaviour
{
	public UIDocument document;

	private VisualElement _root;

	private void Start()
	{
		_root = document.rootVisualElement;
		VisualElement foldout = _root.Q<Foldout>( "OrbitFoldout" );
		VisualElement content = foldout.Q<VisualElement>( "unity-content" );

		VisualElement testList = new ListView();

		for( int i = 0; i < 10; i++ )
			testList.Add( new Label( i.ToString() ) );

		content.Add( testList );
	}
}