﻿/*
	This software known as 'Distrib' at time of creation is under the GNU GPL v2. License
		This license can be found at: http://www.gnu.org/licenses/gpl-2.0.html

	This software is the sole copyright of Clint Pearson
		Contact: clintkpearson@gmail.com

	These following points are generalisations of the terms of the license and as such you MUST read the license itself
	in order to correctly know your rights and responsibilities.

	Primarily the license states:
		> You ARE ALLOWED to use this software be it in binary form or in source form (whole or part) in any software that is
			1. Free to use (though you may charge for distribution)
			2. Free to study and modify (though you may charge for distribution of the source code)
			3. Available under the same license as this software

		> You ARE NOT ALLOWED to use this software be it in binary form or in source form (whole or part) in any software that is:
			1. Commercial (this covers any software for which there is a fee to use and procure)
			2. Software that is closed-source (even if the source is available for a fee)
			3. Not available under the same license as this software

	If this software has been made available to you under any license other than the original license by any party other than the
	original copyright holder (Clint Pearson) then they have acted under breach of their original agreement.

	If this software has been made available to you for a fee for distribution by any party other than the original copyright holder (Clint Pearson)
	then they have acted under breach of their original agreement unless this software is a derivative created by that party.

	If you have received this software from the original copyright holder (Clint Pearson) and it has been made available to you under
	the terms of the original license and you wish to obtain a different license to cover your use of the software, then you may contact
	the copyright holder to negotiate a new license.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distrib.Utils
{
    public sealed class LO<T> : IDisposable
    {
        private T m_value = default(T);
        private ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public LO(T initialValue = default(T))
        {
            m_value = initialValue;
        }

        public T Value
        {
            get
            {
                bool bRead = false;
                try
                {
                    m_lock.EnterReadLock();
                    bRead = true;
                    return m_value;
                }
                finally
                {
                    if (bRead)
                    {
                        m_lock.ExitReadLock();
                    }
                }
            }

            set
            {
                bool bWritten = false;
                try
                {
                    m_lock.EnterWriteLock();
                    m_value = value;
                    bWritten = true;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Failed to set value", ex);
                }
                finally
                {
                    if (bWritten)
                    {
                        m_lock.ExitWriteLock();
                    }
                }
            }
        }

        public static implicit operator T(LO<T> lockObj)
        {
            return lockObj.Value;
        }

        public void Dispose()
        {
            if (m_lock != null)
            {
                m_lock.Dispose();
            }
        }
    }
}
