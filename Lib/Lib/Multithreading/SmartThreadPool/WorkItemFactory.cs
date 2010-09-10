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

namespace sones.Lib.Threading.Internal
{
	#region WorkItemFactory class 

	public class WorkItemFactory
	{
		/// <summary>
		/// Create a new work item
		/// </summary>
		/// <param name="workItemsGroup">The WorkItemsGroup of this workitem</param>
		/// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="callback">A callback to execute</param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemCallback callback)
		{
			return CreateWorkItem(workItemsGroup, wigStartInfo, callback, null);
		}

		/// <summary>
		/// Create a new work item
		/// </summary>
        /// <param name="workItemsGroup">The WorkItemsGroup of this workitem</param>
        /// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="callback">A callback to execute</param>
		/// <param name="workItemPriority">The priority of the work item</param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemCallback callback, 
			WorkItemPriority workItemPriority)
		{
			return CreateWorkItem(workItemsGroup, wigStartInfo, callback, null, workItemPriority);
		}

		/// <summary>
		/// Create a new work item
		/// </summary>
        /// <param name="workItemsGroup">The WorkItemsGroup of this workitem</param>
        /// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="workItemInfo">Work item info</param>
		/// <param name="callback">A callback to execute</param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemInfo workItemInfo, 
			WorkItemCallback callback)
		{
			return CreateWorkItem(
				workItemsGroup,
				wigStartInfo,
				workItemInfo, 
				callback, 
				null);
		}

		/// <summary>
		/// Create a new work item
		/// </summary>
        /// <param name="workItemsGroup">The WorkItemsGroup of this workitem</param>
        /// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="callback">A callback to execute</param>
		/// <param name="state">
		/// The context object of the work item. Used for passing arguments to the work item. 
		/// </param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemCallback callback, 
			object state)
		{
			ValidateCallback(callback);
            
			WorkItemInfo workItemInfo = new WorkItemInfo();
			workItemInfo.UseCallerCallContext = wigStartInfo.UseCallerCallContext;
			workItemInfo.UseCallerHttpContext = wigStartInfo.UseCallerHttpContext;
			workItemInfo.PostExecuteWorkItemCallback = wigStartInfo.PostExecuteWorkItemCallback;
			workItemInfo.CallToPostExecute = wigStartInfo.CallToPostExecute;
			workItemInfo.DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects;
            workItemInfo.WorkItemPriority = wigStartInfo.WorkItemPriority;

			WorkItem workItem = new WorkItem(
				workItemsGroup,
				workItemInfo,
				callback, 
				state);
			return workItem;
		}

		/// <summary>
		/// Create a new work item
		/// </summary>
		/// <param name="workItemsGroup">The work items group</param>
		/// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="callback">A callback to execute</param>
		/// <param name="state">
		/// The context object of the work item. Used for passing arguments to the work item. 
		/// </param>
		/// <param name="workItemPriority">The work item priority</param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemCallback callback, 
			object state, 
			WorkItemPriority workItemPriority)
		{
			ValidateCallback(callback);

			WorkItemInfo workItemInfo = new WorkItemInfo();
			workItemInfo.UseCallerCallContext = wigStartInfo.UseCallerCallContext;
			workItemInfo.UseCallerHttpContext = wigStartInfo.UseCallerHttpContext;
			workItemInfo.PostExecuteWorkItemCallback = wigStartInfo.PostExecuteWorkItemCallback;
			workItemInfo.CallToPostExecute = wigStartInfo.CallToPostExecute;
			workItemInfo.DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects;
			workItemInfo.WorkItemPriority = workItemPriority;

			WorkItem workItem = new WorkItem(
				workItemsGroup,
				workItemInfo,
				callback, 
				state);

			return workItem;
		}

		/// <summary>
		/// Create a new work item
		/// </summary>
        /// <param name="workItemsGroup">The work items group</param>
		/// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="workItemInfo">Work item information</param>
		/// <param name="callback">A callback to execute</param>
		/// <param name="state">
		/// The context object of the work item. Used for passing arguments to the work item. 
		/// </param>
		/// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(
            IWorkItemsGroup workItemsGroup,
            WIGStartInfo wigStartInfo,
            WorkItemInfo workItemInfo,
            WorkItemCallback callback,
            object state)
        {
            ValidateCallback(callback);
            ValidateCallback(workItemInfo.PostExecuteWorkItemCallback);

            WorkItem workItem = new WorkItem(
                workItemsGroup,
                new WorkItemInfo(workItemInfo),
                callback,
                state);

            return workItem;
        }

		/// <summary>
		/// Create a new work item
		/// </summary>
        /// <param name="workItemsGroup">The work items group</param>
		/// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="callback">A callback to execute</param>
		/// <param name="state">
		/// The context object of the work item. Used for passing arguments to the work item. 
		/// </param>
		/// <param name="postExecuteWorkItemCallback">
		/// A delegate to call after the callback completion
		/// </param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemCallback callback, 
			object state,
			PostExecuteWorkItemCallback postExecuteWorkItemCallback)
		{
			ValidateCallback(callback);
			ValidateCallback(postExecuteWorkItemCallback);

			WorkItemInfo workItemInfo = new WorkItemInfo();
			workItemInfo.UseCallerCallContext = wigStartInfo.UseCallerCallContext;
			workItemInfo.UseCallerHttpContext = wigStartInfo.UseCallerHttpContext;
			workItemInfo.PostExecuteWorkItemCallback = postExecuteWorkItemCallback;
			workItemInfo.CallToPostExecute = wigStartInfo.CallToPostExecute;
			workItemInfo.DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects;
            workItemInfo.WorkItemPriority = wigStartInfo.WorkItemPriority;

			WorkItem workItem = new WorkItem(
				workItemsGroup,
				workItemInfo,
				callback, 
				state);

			return workItem;
		}

		/// <summary>
		/// Create a new work item
		/// </summary>
        /// <param name="workItemsGroup">The work items group</param>
		/// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="callback">A callback to execute</param>
		/// <param name="state">
		/// The context object of the work item. Used for passing arguments to the work item. 
		/// </param>
		/// <param name="postExecuteWorkItemCallback">
		/// A delegate to call after the callback completion
		/// </param>
		/// <param name="workItemPriority">The work item priority</param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemCallback callback, 
			object state,
			PostExecuteWorkItemCallback postExecuteWorkItemCallback,
			WorkItemPriority workItemPriority)
		{
			ValidateCallback(callback);
			ValidateCallback(postExecuteWorkItemCallback);

			WorkItemInfo workItemInfo = new WorkItemInfo();
			workItemInfo.UseCallerCallContext = wigStartInfo.UseCallerCallContext;
			workItemInfo.UseCallerHttpContext = wigStartInfo.UseCallerHttpContext;
			workItemInfo.PostExecuteWorkItemCallback = postExecuteWorkItemCallback;
			workItemInfo.CallToPostExecute = wigStartInfo.CallToPostExecute;
			workItemInfo.DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects;
			workItemInfo.WorkItemPriority = workItemPriority;

			WorkItem workItem = new WorkItem(
				workItemsGroup,
				workItemInfo,
				callback, 
				state);

			return workItem;
		}

		/// <summary>
		/// Create a new work item
		/// </summary>
        /// <param name="workItemsGroup">The work items group</param>
		/// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="callback">A callback to execute</param>
		/// <param name="state">
		/// The context object of the work item. Used for passing arguments to the work item. 
		/// </param>
		/// <param name="postExecuteWorkItemCallback">
		/// A delegate to call after the callback completion
		/// </param>
		/// <param name="callToPostExecute">Indicates on which cases to call to the post execute callback</param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemCallback callback, 
			object state,
			PostExecuteWorkItemCallback postExecuteWorkItemCallback,
			CallToPostExecute callToPostExecute)
		{
			ValidateCallback(callback);
			ValidateCallback(postExecuteWorkItemCallback);

			WorkItemInfo workItemInfo = new WorkItemInfo();
			workItemInfo.UseCallerCallContext = wigStartInfo.UseCallerCallContext;
			workItemInfo.UseCallerHttpContext = wigStartInfo.UseCallerHttpContext;
			workItemInfo.PostExecuteWorkItemCallback = postExecuteWorkItemCallback;
			workItemInfo.CallToPostExecute = callToPostExecute;
			workItemInfo.DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects;
            workItemInfo.WorkItemPriority = wigStartInfo.WorkItemPriority;

			WorkItem workItem = new WorkItem(
				workItemsGroup,
				workItemInfo,
				callback, 
				state);

			return workItem;
		}

		/// <summary>
		/// Create a new work item
		/// </summary>
        /// <param name="workItemsGroup">The work items group</param>
		/// <param name="wigStartInfo">Work item group start information</param>
		/// <param name="callback">A callback to execute</param>
		/// <param name="state">
		/// The context object of the work item. Used for passing arguments to the work item. 
		/// </param>
		/// <param name="postExecuteWorkItemCallback">
		/// A delegate to call after the callback completion
		/// </param>
		/// <param name="callToPostExecute">Indicates on which cases to call to the post execute callback</param>
		/// <param name="workItemPriority">The work item priority</param>
		/// <returns>Returns a work item</returns>
		public static WorkItem CreateWorkItem(
			IWorkItemsGroup workItemsGroup,
			WIGStartInfo wigStartInfo,
			WorkItemCallback callback, 
			object state,
			PostExecuteWorkItemCallback postExecuteWorkItemCallback,
			CallToPostExecute callToPostExecute,
			WorkItemPriority workItemPriority)
		{

			ValidateCallback(callback);
			ValidateCallback(postExecuteWorkItemCallback);

			WorkItemInfo workItemInfo = new WorkItemInfo();
			workItemInfo.UseCallerCallContext = wigStartInfo.UseCallerCallContext;
			workItemInfo.UseCallerHttpContext = wigStartInfo.UseCallerHttpContext;
			workItemInfo.PostExecuteWorkItemCallback = postExecuteWorkItemCallback;
			workItemInfo.CallToPostExecute = callToPostExecute;
			workItemInfo.WorkItemPriority = workItemPriority;
			workItemInfo.DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects;

			WorkItem workItem = new WorkItem(
				workItemsGroup,
				workItemInfo,
				callback, 
				state);
			
			return workItem;
		}

		private static void ValidateCallback(Delegate callback)
		{
			if(callback.GetInvocationList().Length > 1)
			{
				throw new NotSupportedException("SmartThreadPool doesn't support delegates chains");
			}
		}
	}

	#endregion
}
