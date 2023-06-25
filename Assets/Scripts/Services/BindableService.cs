using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Services
{
    public class BindableService : MonoBehaviour
    {
        public static void AutoBind<T>(Component origin)
        {
            T target = origin.GetComponentInChildren<T>(true);
            if (target == null)
                return;

            origin.GetComponentsInChildren<IBindable<T>>(true)
                .ToList()
                .ForEach(x => x.Bind(target));
        }
    }
}
