using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Tomia.Builder;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Tomia.Samples
{
	public class Test : MonoBehaviour
	{
		private readonly StringBuilder _writeBuffer = new StringBuilder();
		private readonly StringBuilder _errorBuffer = new StringBuilder();
		
		private void Start()
		{
			// ID sanity checks, if these don't return the expected values then we need to re check how de decide ID's
			
			ForeignClass.DefaultAlloc<Collider>();
			ForeignClass.DefaultAlloc<Collider>();
			ForeignClass.DefaultAlloc<BoxCollider>();
			ForeignClass.DefaultAlloc<BoxCollider>();
			
			ForeignClass.DefaultAlloc<Collider>();
			ForeignClass.DefaultAlloc<Test>();
		}
	}
}
