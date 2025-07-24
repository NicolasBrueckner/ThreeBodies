#region

using System.Collections.Generic;
using UnityEngine;

#endregion

[ CreateAssetMenu( fileName = "New Sequence List", menuName = "Create New Sequence List" ) ]
public class SequenceList : ScriptableObject
{
	public List<string> sequences;
}