/*
* sones GraphDB - Open Source Edition - http://www.sones.com
* Copyright (C) 2007-2010 sones GmbH
*
* This file is part of sones GraphDB Open Source Edition (OSE).
*
* sones GraphDB OSE is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
* 
* sones GraphDB OSE is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB OSE. If not, see <http://www.gnu.org/licenses/>.
* 
*/

using System;
using System.Collections;
using System.Diagnostics;

namespace sones.Lib.Threading.Internal
{
	#region PriorityQueue class

	/// <summary>
	/// PriorityQueue class
	/// This class is not thread safe because we use external lock
	/// </summary>
	public sealed class PriorityQueue : IEnumerable
	{
		#region Private members

		/// <summary>
		/// The number of queues, there is one for each type of priority
		/// </summary>
		private const int _queuesCount = WorkItemPriority.Highest-WorkItemPriority.Lowest+1;

		/// <summary>
		/// Work items queues. There is one for each type of priority
		/// </summary>
		private readonly Queue [] _queues = new Queue[_queuesCount];

		/// <summary>
		/// The total number of work items within the queues 
		/// </summary>
		private int _workItemsCount;

		/// <summary>
		/// Use with IEnumerable interface
		/// </summary>
		private int _version;

		#endregion

		#region Contructor

		public PriorityQueue()
		{
			for(int i = 0; i < _queues.Length; ++i)
			{
				_queues[i] = new Queue();
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Enqueue a work item.
		/// </summary>
		/// <param name="workItem">A work item</param>
		public void Enqueue(IHasWorkItemPriority workItem)
		{
			Debug.Assert(null != workItem);

			int queueIndex = _queuesCount-(int)workItem.WorkItemPriority-1;
			Debug.Assert(queueIndex >= 0);
			Debug.Assert(queueIndex < _queuesCount);

			_queues[queueIndex].Enqueue(workItem);
			++_workItemsCount;
			++_version;
		}

		/// <summary>
		/// Dequeque a work item.
		/// </summary>
		/// <returns>Returns the next work item</returns>
		public IHasWorkItemPriority Dequeue()
		{
			IHasWorkItemPriority workItem = null;

			if(_workItemsCount > 0)
			{
				int queueIndex = GetNextNonEmptyQueue(-1);
				Debug.Assert(queueIndex >= 0);
				workItem = _queues[queueIndex].Dequeue() as IHasWorkItemPriority;
				Debug.Assert(null != workItem);
				--_workItemsCount;
				++_version;
			}

			return workItem;
		}

		/// <summary>
		/// Find the next non empty queue starting at queue queueIndex+1
		/// </summary>
		/// <param name="queueIndex">The index-1 to start from</param>
		/// <returns>
		/// The index of the next non empty queue or -1 if all the queues are empty
		/// </returns>
		private int GetNextNonEmptyQueue(int queueIndex)
		{
			for(int i = queueIndex+1; i < _queuesCount; ++i)
			{
				if(_queues[i].Count > 0)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// The number of work items 
		/// </summary>
		public int Count
		{
			get
			{
				return _workItemsCount;
			}
		}

		/// <summary>
		/// Clear all the work items 
		/// </summary>
		public void Clear()
		{
			if (_workItemsCount > 0)
			{
				foreach(Queue queue in _queues)
				{
					queue.Clear();
				}
				_workItemsCount = 0;
				++_version;
			}
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator to iterate over the work items
		/// </summary>
		/// <returns>Returns an enumerator</returns>
		public IEnumerator GetEnumerator()
		{
			return new PriorityQueueEnumerator(this);
		}

		#endregion

		#region PriorityQueueEnumerator

		/// <summary>
		/// The class the implements the enumerator
		/// </summary>
		private class PriorityQueueEnumerator : IEnumerator
		{
			private readonly PriorityQueue _priorityQueue;
			private int _version;
			private int _queueIndex;
			private IEnumerator _enumerator;

			public PriorityQueueEnumerator(PriorityQueue priorityQueue)
			{
				_priorityQueue = priorityQueue;
				_version = _priorityQueue._version;
				_queueIndex = _priorityQueue.GetNextNonEmptyQueue(-1);
				if (_queueIndex >= 0)
				{
					_enumerator = _priorityQueue._queues[_queueIndex].GetEnumerator();
				}
				else
				{
					_enumerator = null;
				}
			}

			#region IEnumerator Members

			public void Reset()
			{
				_version = _priorityQueue._version;
				_queueIndex = _priorityQueue.GetNextNonEmptyQueue(-1);
				if (_queueIndex >= 0)
				{
					_enumerator = _priorityQueue._queues[_queueIndex].GetEnumerator();
				}
				else
				{
					_enumerator = null;
				}
			}

			public object Current
			{
				get
				{
					Debug.Assert(null != _enumerator);
					return _enumerator.Current;
				}
			}

			public bool MoveNext()
			{
				if (null == _enumerator)
				{
					return false;
				}

				if(_version != _priorityQueue._version)
				{
					throw new InvalidOperationException("The collection has been modified");

				}
				if (!_enumerator.MoveNext())
				{
					_queueIndex = _priorityQueue.GetNextNonEmptyQueue(_queueIndex);
					if(-1 == _queueIndex)
					{
						return false;
					}
					_enumerator = _priorityQueue._queues[_queueIndex].GetEnumerator();
					_enumerator.MoveNext();
					return true;
				}
				return true;
			}

			#endregion
		}

		#endregion
	}

	#endregion
}
