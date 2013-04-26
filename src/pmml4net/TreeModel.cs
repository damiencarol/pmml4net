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
	/// Description of TreeModel.
	/// </summary>
	public class TreeModel
	{
		private String modelName;
		private MiningSchema miningSchema;
		private Node node;
		
		/// <summary>
		/// Identifies the model with a unique name in the context of the PMML file.
		/// </summary>
		public String ModelName { get { return modelName; } set { modelName = value; } }
		
		/// <summary>
		/// Mining schema for this model.
		/// </summary>
		public MiningSchema MiningSchema { get { return miningSchema; } set { miningSchema = value; } }
		
		/// <summary>
		/// Root node of this model.
		/// </summary>
		public Node Node { get { return node; } set { node = value; } }
		
		/// <summary>
		/// Scoring with Tree Model
		/// </summary>
		/// <param name="dict">Values</param>
		/// <returns></returns>
		public ScoreResult Score(Dictionary<string, object> dict)
		{
			ScoreResult res = new ScoreResult("XXXXXXXX", "XXXXXXXXX");
			
			// evaluate nodes
			node.Evaluate(dict, res);
			
			if (res.Nodes.Count > 0)
				res.Value = res.Nodes[res.Nodes.Count - 1].Score;
			
			return res;
		}
		
		/// <summary>
		/// Load tree model from xmlnode
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static TreeModel loadFromXmlNode(XmlNode node)
		{
			TreeModel tree = new TreeModel();
			
			tree.ModelName = node.Attributes["modelName"].Value;
			
			foreach(XmlNode item in node.ChildNodes)
			{
				if ("Node".Equals(item.Name))
				{
					tree.Node = Node.loadFromXmlNode(item);
				}
			}
			
			return tree;
		}
	}
}
