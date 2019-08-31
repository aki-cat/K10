﻿using System.Collections.Generic;
using UnityEditor;

namespace K10
{
	namespace EditorGUIExtention
	{
		public class EditorGuiIndentManager
		{
			static List<int> _widths = new List<int>();

			public static void New( int indent )
			{
				_widths.Add( EditorGUI.indentLevel );
				EditorGUI.indentLevel = indent;
			}

			public static void Revert()
			{
				if( _widths.Count > 0 )
				{
					EditorGUI.indentLevel = _widths[_widths.Count - 1];
					_widths.RemoveAt( _widths.Count - 1 );
				}
			}
		}
	}
}
