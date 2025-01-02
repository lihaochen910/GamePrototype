#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;

namespace Pixpil.RPGStatSystem {

	[CreateAssetMenu ( menuName = "Pixpil/RPGStatCollectionDefine" )]
	public class RPGStatCollectionDefine : ScriptableObject {

		[SerializeField]
		private RPGStatDefineData[] datas;

		public RPGStatDefineData[] getDefinedCollection () => datas;

		public enum RPGStatType : byte {
			RPGStat,
			RPGStatModifiable,
			RPGAttribute,
			RPGVital
		}

		[Serializable]
		public class RPGStatDefineData {

			[SerializeField]
			public string key;

			[SerializeField]
			public RPGStatType type;

			[SerializeField]
			public float defaultValue;
		}
	}
}
#endif
