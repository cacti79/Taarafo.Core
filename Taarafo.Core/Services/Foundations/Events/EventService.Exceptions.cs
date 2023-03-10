﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Taarafo.Core.Models.Events;
using Taarafo.Core.Models.Events.Exceptions;
using Xeptions;

namespace Taarafo.Core.Services.Foundations.Events
{
    public partial class EventService
    {
        private delegate IQueryable<Event> ReturningEventsFunction();
        private delegate ValueTask<Event> ReturningEventFunction();

        private IQueryable<Event> TryCatch(ReturningEventsFunction returningEventsFunction)
        {
            try
            {
                return returningEventsFunction();
            }
            catch (SqlException sqlException)
            {
                var failedEventStorageException = new FailedEventStorageException(sqlException);

                throw CreateAndLogCriticalDependencyException(failedEventStorageException);
            }
            catch (Exception exception)
            {
                var failedEventServiceException =
                    new FailedEventServiceException(exception);

                throw CreateAndLogServiceException(failedEventServiceException);
            }
        }

        private async ValueTask<Event> TryCatch(ReturningEventFunction returningEventFunction)
        {
            try
            {
                return await returningEventFunction();
            }
            catch (NullEventException nullEventException)
            {
                throw CreateAndLogValidationException(nullEventException);
            }
        }

       private EventValidationException CreateAndLogValidationException(
            Xeption exception)
        {
            var eventValidationException =
                new EventValidationException(exception);

            this.loggingBroker.LogError(eventValidationException);

            return eventValidationException;
        }

        private EventDependencyException CreateAndLogCriticalDependencyException(Xeption exception)
        {
            var eventDependencyException = new EventDependencyException(exception);
            this.loggingBroker.LogCritical(eventDependencyException);

            return eventDependencyException;
        }

        private EventServiceException CreateAndLogServiceException(Xeption exception)
        {
            var eventServiceException = new EventServiceException(exception);
            this.loggingBroker.LogError(eventServiceException);

            return eventServiceException;
        }
    }
}
