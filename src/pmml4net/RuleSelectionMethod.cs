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
using System.Globalization;

namespace pmml4net
{
	/// <summary>
	/// Description of RuleSelectionMethod.
	/// </summary>
	public class RuleSelectionMethod
	{
		private string criterion;
		
		/// <summary>
		/// 
		/// </summary>
		public RuleSelectionMethod(string criterion)
		{
			this.criterion = criterion;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public string Criterion { get { return criterion; } set { criterion = value; } }
		
		/// <summary>
		/// Load Node from XmlNode
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static RuleSelectionMethod loadFromXmlNode(XmlNode node)
		{
			
			//if (node.Attributes["criterion"] != null)
			//	root.criterion = ;
			
			RuleSelectionMethod root = new RuleSelectionMethod(node.Attributes["criterion"].Value);
			
			/*if (node.Attributes["score"] != null)
				root.score = node.Attributes["score"].Value;
			
			if (node.Attributes["recordCount"] != null)
				root.recordCount = Convert.ToDecimal(node.Attributes["recordCount"].Value, CultureInfo.InvariantCulture);
			
			root.scoreDistributions = new List<ScoreDistribution>();*/
			foreach(XmlNode item in node.ChildNodes)
			{
				if ("extension".Equals(item.Name.ToLowerInvariant()))
				{
					// TODO : implement extension
					//root.Nodes.Add(Node.loadFromXmlNode(item));
				}
				/*else if ("node".Equals(item.Name.ToLowerInvariant()))
				{
					root.Nodes.Add(Node.loadFromXmlNode(item));
				}*/
				/*else if ("simplepredicate".Equals(item.Name.ToLowerInvariant()))
				{
					root.Predicate = SimplePredicate.loadFromXmlNode(item);
				}
				else if ("true".Equals(item.Name.ToLowerInvariant()))
				{
					root.Predicate = new TruePredicate();
				}
				else if ("false".Equals(item.Name.ToLowerInvariant()))
				{
					root.Predicate = new FalsePredicate();
				}
				else if ("compoundpredicate".Equals(item.Name.ToLowerInvariant()))
				{
					root.Predicate = CompoundPredicate.loadFromXmlNode(item);
				}
				else if ("simplesetpredicate".Equals(item.Name.ToLowerInvariant()))
				{
					root.Predicate = SimpleSetPredicate.loadFromXmlNode(item);
				}
				else if ("scoredistribution".Equals(item.Name.ToLowerInvariant()))
				{
					root.ScoreDistributions.Add(ScoreDistribution.loadFromXmlNode(item));
				}*/
				else
					throw new NotImplementedException();
			}
			
			return root;
		}
		
		/// <summary>
		/// Scoring with Tree Model
		/// </summary>
		/// <param name="root">Parent node</param>
		/// <param name="missingvalueStr">Missing value strategy to evaluate this node.</param>
		/// <param name="noTrueChildStr">Strategy to evaluate this node if no child are true</param>
		/// <param name="dict">Values</param>
		/// <param name="res" >Result to return</param>
		/// <returns></returns>
		public static ScoreResult Evaluate(Node root, MissingValueStrategy missingvalueStr, NoTrueChildStrategy noTrueChildStr, 
		                            Dictionary<string, object> dict, ScoreResult res)
		{
			// Test childs
			foreach(Node child in root.Nodes)
			{
				PredicateResult childPredicate = child.Predicate.Evaluate(dict);
				if (childPredicate == PredicateResult.True)
				{
					res.Nodes.Add(child);
					res.Value = child.Score;
					foreach(ScoreDistribution sco in child.ScoreDistributions)
					{
						if (sco.Value.Equals(child.Score))
						{
							if (sco.Confidence != null)
								res.Confidence = Decimal.Parse(sco.Confidence, NumberStyles.Any, CultureInfo.InvariantCulture);
						}
					}
					
					return Evaluate(child, missingvalueStr, noTrueChildStr, dict, res);
				}
				else if (childPredicate == PredicateResult.Unknown)
				{
					// Unknow value lead to act with missingvalueStr
					switch(missingvalueStr)
					{
						case MissingValueStrategy.LastPrediction:
							return res;
							
						case MissingValueStrategy.NullPrediction:
							res.Value = null;
							return res;
							
						case MissingValueStrategy.WeightedConfidence:
							Dictionary<string, decimal> conf = CalculateConfidence(root, dict);
							string max_conf = null;
							foreach(string key in conf.Keys)
							{
								if (max_conf == null)
									max_conf = key;
								
								if (conf[key] > conf[max_conf])
									max_conf = key;
							}
							res.Value = max_conf;
							res.Confidence = conf[max_conf];
							return res;
							
						case MissingValueStrategy.AggregateNodes:
							return res;
							
						default:
							throw new NotImplementedException();
					}
				}
			}
			
			// All child nodes are false
			if (root.Nodes.Count > 0)
				if (noTrueChildStr == NoTrueChildStrategy.ReturnNullPrediction)
				{
					res.Value = null;
				}
			return res;
		}
		
		private static Dictionary<String, decimal> CalculateConfidence(Node node, Dictionary<string, object> dict)
		{
			Dictionary<String, decimal> ret = new Dictionary<string, decimal>();
			
			// Test childs
			foreach(Node child in node.Nodes)
			{
				PredicateResult childPredicate = child.Predicate.Evaluate(dict);
				if (childPredicate != PredicateResult.False)
				{
					foreach(ScoreDistribution sd in child.ScoreDistributions)
					{
						if (!ret.ContainsKey(sd.Value))
							ret.Add(sd.Value, 0);
						
						decimal new_val = (Convert.ToDecimal(sd.RecordCount)/child.RecordCount) * (child.RecordCount / node.RecordCount);
						ret[sd.Value] = ret[sd.Value] + new_val;
					}
				}
			}
			
			return ret;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void save(XmlWriter writer)
		{
			writer.WriteStartElement("RuleSelectionMethod");
			
			writer.WriteAttributeString("criterion", this.criterion);
			
			writer.WriteEndElement();
		}
	}
}
