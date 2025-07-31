#region

using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class UIController : MonoBehaviour
{
	public OrbitInformationLoader loader;
	public TMP_Dropdown orbitDropdown;
	public TMP_Dropdown sequenceDropdown;
	public Button settingButton;
	public Button hideButton;
	public SequenceList sequences;

	[ Header( "Information Labels" ) ]
	public TMP_Text positionLabel;

	public TMP_Text velocityLabel;
	public TMP_Text massLabel;
	public TMP_Text freeGroupLabel;
	public TMP_Text periodLabel;
	public TMP_Text energyLabel;
	public TMP_Text dateLabel;


	private void Start()
	{
		RuntimeEventManager.FileLoaded += OnFileLoaded;
		RuntimeEventManager.OrbitInfoLoaded += OnInfoLoaded;

		sequenceDropdown.onValueChanged.AddListener( OnSequenceChanged );
		orbitDropdown.onValueChanged.AddListener( OnOrbitChanged );

		sequenceDropdown.ClearOptions();
		sequenceDropdown.AddOptions( sequences.sequences );

		OnSequenceChanged( 0 );
	}

	private void OnInfoLoaded( OrbitInformation obj )
	{
		WriteInformation( obj );
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

	private void OnFileLoaded()
	{
		orbitDropdown.ClearOptions();
		orbitDropdown.AddOptions( loader.GetKeys() );
	}

	private void WriteInformation( OrbitInformation obj )
	{
		positionLabel.text = "Initial positions: [" +
		                     string.Join( ", ", obj.initialPositions.Select( v => $"({v.x}, {v.y})" ) ) + "]";
		velocityLabel.text = "Initial velocities: [" +
		                     string.Join( ", ", obj.initialVelocities.Select( v => $"({v.x}, {v.y})" ) ) + "]";
		massLabel.text = $"Masses: {obj.masses}";
		freeGroupLabel.text = $"Free group element: {obj.freeGroupElement}";
		periodLabel.text = $"Period: {obj.period}";
		energyLabel.text = $"Energy: {obj.energy}";
		dateLabel.text = $"Date discovered: {obj.year}";
	}
}