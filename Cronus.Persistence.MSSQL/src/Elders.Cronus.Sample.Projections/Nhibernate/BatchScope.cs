﻿using System;
using NHibernate;
using Elders.Cronus.Messaging.MessageHandleScope;

namespace Elders.Cronus.Sample.Handlers.Nhibernate
{
    public class BatchScope : IBatchScope
    {
        private readonly ISessionFactory sessionFactory;
        private ISession session;
        private ITransaction transaction;

        public BatchScope(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Begin()
        {
            Lazy<ISession> lazySession = new Lazy<ISession>(() =>
            {
                session = sessionFactory.OpenSession();

                transaction = session.BeginTransaction();
                return session;
            });
            Context.Set<Lazy<ISession>>(lazySession);
        }

        public void End()
        {
            if (session != null)
            {
                try { transaction.Commit(); }
                catch (Exception)
                {
                    if (transaction.IsActive && transaction.WasCommitted)
                        transaction.Rollback();
                    throw;
                }
                finally
                {
                    session.Clear();
                    session.Close();
                }
                session = null;
            }
        }

        public IScopeContext Context { get; set; }
    }
}