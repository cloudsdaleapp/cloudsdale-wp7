using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cloudsdale.Model
{
	[JsonObject(MemberSerialization.OptIn)]
	class CloudsdaleModel : INotifyPropertyChanged
	{
		#region >> CONSTRUCTOR

		public CloudsdaleModel()
		{
			UIMetadata = new UIMetadata(this);
		}

		#endregion

		#region >> PROPERTIES

		public UIMetadata UIMetadata { get; set; }
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region >> METHODS

		[NotifyPropertyChangedInvocator]
		protected internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler == null) return;
			if (ModelSettings.Dispatcher != null
			    && ModelSettings.Dispatcher.HasThreadAccess)
			{
				ModelSettings.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
					() => handler(this, new PropertyChangedEventArgs(propertyName)));
			}
			else
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[OnError]
		public void OnError(StreamingContext context, ErrorContext errorContext)
		{
			errorContext.Handled = true;
			Debugger.Break();
		}

		#endregion
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class JsonErrorValueAttribute : Attribute
	{
		#region >> PROPERTIES

		public object DefaultValue { get; set; }

		#endregion

		#region >> METHODS

		public JsonErrorValueAttribute(object defaultValue)
		{
			DefaultValue = defaultValue;
		}

		#endregion

	}

	[JsonObject(MemberSerialization.OptIn)]
	public class CloudsdaleResource : CloudsdaleModel
	{

		#region >> CONSTRUCTOR

		[JsonConstructor]
		public CloudsdaleResource(string id)
		{
			LastUpdated = DateTime.Now;
			Id = id;
		}

		#endregion

		#region >> PROPERTIES

		/// <summary>
		/// The unique ID of the resource
		/// </summary>
		[JsonProperty("id")]
		public readonly string Id;

		/// <summary>
		/// a boolean value determining if the resource's data could be invalid
		/// </summary>
		public bool Invalidated
		{
			get { return LastUpdated < ModelSettings.AppLastSuspended; }
		}

		#endregion

		#region >> METHODS

		/// <summary>
		/// Determines whether the model is able to
		/// be validated and updated at this time
		/// </summary>
		/// <returns>Whether the model can be updated</returns>
		public virtual bool CanValidate()
		{
			return true;
		}

		/// <summary>
		/// Asyncronously validate the model
		/// </summary>
		/// <returns>Whether the resource is now valid</returns>
		public Task<bool> Validate()
		{
			return Validate(false);
		}

		public async Task<bool> Validate(bool force)
		{
			if (!Invalidated && !force) return true;
			if (!CanValidate()) return false;

			var modelType = GetType().GetTypeInfo();
			var attribute = modelType.GetCustomAttribute<ResourceEndpointAttribute>();

			var requestUrl = attribute.Endpoint.Replace("[:id]", Id);
			var client = new HttpClient
		}

		/// <summary>
		/// Asyncronously validates the model,
		/// ignoring its current validation status
		/// </summary>
		/// <returns></returns>
		public Task<bool> ForceValidate()
		{
			return Validate(true);
		}

		#endregion
	}
}
