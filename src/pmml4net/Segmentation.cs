﻿/*
pmml4net - easy lib to read and consume tree model in PMML file
Copyright (C) 2013  Damien Carol <damien.carol@gmail.com>

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Library General Public
License as published by the Free Software Foundation; either
version 2 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Library General Public License for more details.

You should have received a copy of the GNU Library General Public
License along with this library; if not, write to the
Free Software Foundation, Inc., 51 Franklin St, Fifth Floor,
Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Xml;

namespace pmml4net
{
	/// <summary>
	/// Description of Segmentation.
	/// </summary>
	public class Segmentation
	{
		private MultipleModelMethod multipleModelMethod;
		private IList<Segment> segments;
		
		/// <summary>
		/// 
		/// </summary>
		public MultipleModelMethod MultipleModelMethod { get { return multipleModelMethod; } set { multipleModelMethod = value; } }
		
		/// <summary>
		/// The Segment element is used to tag each model that can be combined as part of an ensemble or associated with a population segment.
		/// </summary>
		public IList<Segment> Segments { get { return segments; } set { segments = value; } }
		
		/// <summary>
		/// Load Segmentation node from XmlElement of PMML file
		/// </summary>
		/// <param name="node">Xml PMML file to read</param>
		/// <returns></returns>
		public static Segmentation loadFromXmlNode(XmlNode node)
		{
			Segmentation segmentation = new Segmentation();
			
			// By default noTrueChildStrategy = returnNullPrediction
			segmentation.multipleModelMethod = MultipleModelMethodfromString(node.Attributes["multipleModelMethod"].Value);
			
			segmentation.segments = new List<Segment>();
			foreach(XmlNode item in node.ChildNodes)
			{
				if ("Extension".Equals(item.Name))
				{
					// Not yet implemented
				}
				else if ("Segment".Equals(item.Name))
				{
					segmentation.Segments.Add(Segment.loadFromXmlNode(item));
				}
				else
					throw new NotImplementedException();
			}
			
			return segmentation;
		}
		
		/// <summary>
		/// Parse <see cref="MultipleModelMethod" >MultipleModelMethod</see> from string.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static MultipleModelMethod MultipleModelMethodfromString(String val)
		{
			switch (val.ToLowerInvariant().Trim())
			{
			case "majorityvote" :
				return MultipleModelMethod.MajorityVote;
			case "weightedmajorityvote" :
				return MultipleModelMethod.WeightedMajorityVote;
			case "average" :
				return MultipleModelMethod.Average;
			case "weightedaverage" :
				return MultipleModelMethod.WeightedAverage;
			case "median" :
				return MultipleModelMethod.Median;
			case "max" :
				return MultipleModelMethod.Max;
			case "sum" :
				return MultipleModelMethod.Sum;
			case "selectfirst" :
				return MultipleModelMethod.SelectFirst;
			case "selectall" :
				return MultipleModelMethod.SelectAll;
			case "modelchain" :
				return MultipleModelMethod.ModelChain;
				
			default:
				throw new NotImplementedException();
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void save(XmlWriter writer)
		{
			writer.WriteStartElement("Segmentation");
			
			writer.WriteAttributeString("multipleModelMethod", MultipleModelMethodToString(this.MultipleModelMethod));
			
			// Save segments
			foreach (Segment segment in this.Segments)
				segment.save(writer);
			
			writer.WriteEndElement();
		}
		
		/// <summary>
		/// Convert MultipleModelMethod to string
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		protected static string MultipleModelMethodToString(MultipleModelMethod val)
		{
			switch (val)
			{
			case MultipleModelMethod.Average:
				return "average";
			case MultipleModelMethod.MajorityVote:
				return "majorityVote";
			case MultipleModelMethod.Max:
				return "max";
			default:
				throw new NotImplementedException();
			}
		}
	}
}
