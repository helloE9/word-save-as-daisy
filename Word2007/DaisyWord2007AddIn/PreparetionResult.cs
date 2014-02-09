﻿using System.Collections;
using Sonata.DaisyConverter.DaisyConverterLib;

namespace DaisyWord2007AddIn
{
	public class PreparetionResult
	{
		public PreparetionResult()
		{
			ObjectShapes = new ArrayList();
			ImageId = new ArrayList();
			InlineShapes = new ArrayList();
			InlineId = new ArrayList();
			//MultipleMathMl = new Hashtable();
			//MultipleOwnMathMl = new Hashtable();
		}

		public static PreparetionResult Failed()
		{
			return new PreparetionResult() { IsSuccess = false };
		}

		public bool IsSuccess { get; set; }
		public string DocxFilePath { get; set; }
		public string DocFilePath { get; set; }
		public Initialize InitializeWindow { get; set; }
		public string MasterSubFlag { get; set; }
		public ArrayList ObjectShapes { get; set; }
		public ArrayList ImageId { get; set; }
		public ArrayList InlineShapes { get; set; }
		public ArrayList InlineId { get; set; }
		//public Hashtable MultipleMathMl { get; set; }
		//public Hashtable MultipleOwnMathMl { get; set; }
	}
}