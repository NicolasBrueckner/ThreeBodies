#region

using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

#endregion

public class UIController : MonoBehaviour
{
	public OrbitInformationLoader loader;
	public SequenceList sequences;

	public TMP_Text fpsDebug;

	[ Header( "Interactive UI Elements" ) ]
	public TMP_Dropdown orbitDropdown;

	public TMP_Dropdown sequenceDropdown;
	public Button settingButton;
	public Button hideButton;
	public Toggle orbitToggle;

	[ Header( "Information Labels" ) ]
	public TMP_Text positionLabel;

	public TMP_Text velocityLabel;
	public TMP_Text massLabel;
	public TMP_Text freeGroupLabel;
	public TMP_Text periodLabel;
	public TMP_Text energyLabel;
	public TMP_Text dateLabel;

	private void Awake()
	{
		RuntimeEventManager.FileLoaded += OnFileLoaded;
		RuntimeEventManager.OrbitInfoLoaded += OnInfoLoaded;
	}

	private void Start()
	{
		sequenceDropdown.onValueChanged.AddListener( OnSequenceChanged );
		orbitDropdown.onValueChanged.AddListener( OnOrbitChanged );
		orbitToggle.onValueChanged.AddListener( OnToggleChanged );

		sequenceDropdown.ClearOptions();
		sequenceDropdown.AddOptions( sequences.sequences );
		sequenceDropdown.value = 11;

		orbitToggle.isOn = false;
	}

	private void Update()
	{
		fpsDebug.text = $"FPS: {Math.Round( 1f / Time.unscaledDeltaTime )}";
	}

	private static void OnToggleChanged( bool isOn )
	{
		RuntimeEventManager.InvokeOrbitToggleChanged( isOn );
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
		positionLabel.text = "Initial positions:\n" +
		                     string.Join( "\n",
			                     obj.initialPositions.Select( v => $"( {v.x,+6:0.000}, {v.y,+6:0.000} )" ) );
		velocityLabel.text = "Initial velocities:\n" +
		                     string.Join( "\n",
			                     obj.initialVelocities.Select( v => $"( {v.x,+6:0.000}, {v.y,+6:0.000} )" ) );
		massLabel.text = "Masses:\n" +
		                 string.Join( ", ", obj.masses.Select( v => $"{v}" ) );
		freeGroupLabel.text = $"Free group element:\n{obj.freeGroupElement}";
		periodLabel.text = $"Period:\n{obj.period}s";
		energyLabel.text = $"Energy:\n{obj.energy}";
		dateLabel.text = $"Date discovered:\n{obj.year}";
	}
}