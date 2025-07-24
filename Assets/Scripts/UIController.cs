#region

using TMPro;
using UnityEngine;

#endregion

public class UIController : MonoBehaviour
{
	public OrbitInformationLoader loader;
	public TMP_Dropdown orbitDropdown;
	public TMP_Dropdown sequenceDropdown;
	public SequenceList sequences;

	private void Start()
	{
		loader.NewFileLoaded += UpdateOrbitDropdown;

		sequenceDropdown.onValueChanged.AddListener( OnSequenceChanged );
		orbitDropdown.onValueChanged.AddListener( OnOrbitChanged );

		sequenceDropdown.ClearOptions();
		sequenceDropdown.AddOptions( sequences.sequences );

		OnSequenceChanged( 0 );
	}

	private void OnOrbitChanged( int index )
	{
		loader.GetInformationByIndex( index );
	}

	private void OnSequenceChanged( int index )
	{
		string fileName = sequences.sequences[ index ];
		loader.LoadNewFile( fileName );
	}

	private void UpdateOrbitDropdown()
	{
		orbitDropdown.ClearOptions();
		orbitDropdown.AddOptions( loader.GetKeys() );
	}
}