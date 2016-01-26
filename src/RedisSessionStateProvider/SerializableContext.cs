using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MainStreet.BusinessFlow.SDK;
using System.Runtime.Serialization;
using System.Collections;

namespace Microsoft.Web.Redis
{
    [Serializable]
    public class SerializableCart
    {
        public SerializableCart()
        { }

        public SerializableCart(Cart cart)
        {
            AgentGuid = cart.AgentGuid;
            ApplicationCode = cart.ApplicationCode;
            CartGuid = cart.CartGuid;
            CartType = cart.CartType;
            Context = new SerializableContext(cart.Context);
            CustomerGuid = cart.CustomerGuid;
            Data = cart.Data;
            Description = cart.Description;
            //DisplayData = cart.DisplayData;
            DoNotPersist = cart.DoNotPersist;
            IsDirty = cart.IsDirty;
            IsDisplayDirty = cart.IsDisplayDirty;
            IsUnused = cart.IsUnused;
            OrderGuid = cart.OrderGuid;
            OrderType = cart.OrderType;
            PaymentGuid = cart.PaymentGuid;
            PaymentOnMethod = cart.PaymentOnMethod;
            PersistTimeout = cart.PersistTimeout;
            ProhibitOversell = cart.ProhibitOversell;
            Tag = cart.Tag;

            MinimalState = cart.GetMinimalState(Cart.MinimalStateModes.PartialData);
        }

        public string MinimalState { get; set; }
        public Guid? AgentGuid { get; set; }
        public string ApplicationCode { get; set; }
        public Guid? CartGuid { get; set; }
        public MainStreet.BusinessFlow.SDK.Ws.CartTypes CartType { get; set; }
        public SerializableContext Context { get; set; }
        public Guid? CustomerGuid { get; set; }
        public OrderSync Data { get; set; }
        public string Description { get; set; }
        //public OrderEvaluatedDisplay DisplayData { get; set; }
        public bool DoNotPersist { get; set; }
        public bool IsDirty { get; set; }
        public bool IsDisplayDirty { get; set; }
        public bool IsUnused { get; set; }
        public Guid? OrderGuid { get; set; }
        public MainStreet.BusinessFlow.SDK.Ws.OrderTypes OrderType { get; set; }
        public Guid? PaymentGuid { get; set; }
        public decimal PaymentOnMethod { get; set; }
        public TimeSpan PersistTimeout { get; set; }
        public bool ProhibitOversell { get; set; }
        public string Tag { get; set; }

        public Cart ToCart()
        {
            Cart cart = null;

            try
            {
                cart = new Cart(this.Context.ToBusinessFlowContext(), MinimalState);
            }
            catch
            {
                MainStreet.BusinessFlow.SDK.Web.BusinessFlow.Context.SetDefaultIdentity(this.Context.Identity);
                cart = new Cart(this.Context.ToBusinessFlowContext());
            }

            cart.AgentGuid = this.AgentGuid.HasValue ? this.AgentGuid.Value : Guid.Empty;
            cart.ApplicationCode = this.ApplicationCode;
            cart.CartGuid = this.CartGuid.HasValue ? this.CartGuid.Value : Guid.Empty;
            cart.CartType = this.CartType;
            //cart.Context = this.Context.ToBusinessFlowContext();
            cart.CustomerGuid = this.CustomerGuid.HasValue ? this.CustomerGuid.Value : Guid.Empty;
            //Data = this.Data;
            cart.Description = this.Description;
            //DisplayData = this.DisplayData;
            cart.DoNotPersist = this.DoNotPersist;
            cart.IsDirty = false;
            cart.IsDisplayDirty = this.IsDisplayDirty;
            cart.IsUnused = this.IsUnused;
            cart.OrderGuid = this.OrderGuid.HasValue ? this.OrderGuid.Value : Guid.Empty;
            cart.OrderType = this.OrderType;
            cart.PaymentGuid = this.PaymentGuid.HasValue ? this.PaymentGuid.Value : Guid.Empty;
            cart.PaymentOnMethod = this.PaymentOnMethod;
            cart.PersistTimeout = this.PersistTimeout;
            cart.ProhibitOversell = this.ProhibitOversell;
            cart.Tag = this.Tag;
            
            cart.IsDirty = IsDirty;
            return cart;
        }
    }

    [Serializable]
    public class SerializableContext 
    {
        public SerializableContext()
        { }

        public SerializableContext(IBusinessFlowContext context)
        {
            CacheMode = context.CacheMode;
            Catalog = context.Catalog;
            Credentials = context.Credentials;
            Identity = context.Identity;
            //MemoryCache = new SerializableCache(context.MemoryCache);
            PriceLevelGuid = context.PriceLevelGuid;
            RequestCacheEnabled = context.RequestCacheEnabled;
            ServerBaseUrl = context.ServerBaseUrl;
        }

        public BusinessFlowIdentity Identity { get; set; }
        //public SerializableCache MemoryCache { get; set; }
        public ContextCacheMode CacheMode { get; set; }
        public string Catalog { get; set; }
        public Credentials Credentials { get; set; }
        public Guid? PriceLevelGuid { get; set; }
        public bool RequestCacheEnabled { get; set; }
        public string ServerBaseUrl { get; set; }

        public MainStreet.BusinessFlow.SDK.Web.BusinessFlowWebContext ToBusinessFlowContext()
        {
            return new MainStreet.BusinessFlow.SDK.Web.BusinessFlowWebContext
            {
                Identity = Identity,
                CacheMode = CacheMode,
                Catalog = Catalog,
                Credentials = Credentials,
                PriceLevelGuid = PriceLevelGuid,
                RequestCacheEnabled = RequestCacheEnabled,
                ServerBaseUrl = ServerBaseUrl
            };
        }
    }

    [Serializable]
    public class SerializableCache : List<KeyValuePair<string, object>>
    {
        public SerializableCache()
        { }

        public SerializableCache(System.Web.Caching.Cache cache)
        {
            foreach(DictionaryEntry item in cache)
            {
                Add(new KeyValuePair<string, object>(item.Key.ToString(), item.Value));
            }
        }
    }
}
