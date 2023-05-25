using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class futuristicComboLockScript : MonoBehaviour {

   public KMBombModule Module;
   public KMBombInfo Bomb;
   public KMAudio Audio;
   public KMSelectable ModuleSelectable;

   public KMSelectable[] Wedges;
   public MeshRenderer[] WedgeMeshes;
   public Material[] Materials;
   public TextMesh Digits;

   private int Number;
   private string CorrectSequence = "";
   private bool Inputting = false;
   private string GivenSequence = "";
   private int[] Shading = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

   static int ModuleIDCounter = 1;
   int ModuleID;
   private bool ModuleSolved = false;

   void Awake () {
      ModuleID = ModuleIDCounter++;
      Module.OnActivate += delegate () { Generate(); };
      ModuleSelectable.OnDefocus += delegate () { Submit(); };
      foreach (KMSelectable Wedge in Wedges) {
         Wedge.OnInteract += delegate () { WedgePress(Wedge); return false; };
         Wedge.OnHighlight += delegate () { WedgeHover(Wedge); };
      }
   }

   void Start () {
      Number = Rnd.Range(0, 99);
      Digits.text = Number.ToString("D2");
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Digits are {1}.", ModuleID, Number.ToString("D2"));
      StartCoroutine(Spin());
   }

   void Generate () {
      string DigitSequence = "";
      DigitSequence += (Number/10).ToString();
      Debug.LogFormat("[Futuristic Combo Lock #{0}] First digit on the module: {1}", ModuleID, Number / 10);
      DigitSequence += Bomb.GetModuleNames().Count.ToString();
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Number of modules on the bomb: {1}", ModuleID, Bomb.GetModuleNames().Count);
      float Minutes = Bomb.GetTime() / 60;
      DigitSequence += ((int)Minutes).ToString();
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Number of minutes on the bomb: {1}", ModuleID, (int)Minutes);
      DigitSequence += Bomb.GetBatteryCount().ToString();
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Number of batteries on the bomb: {1}", ModuleID, Bomb.GetBatteryCount());
      DigitSequence += Bomb.GetBatteryHolderCount().ToString();
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Number of battery holders on the bomb: {1}", ModuleID, Bomb.GetBatteryHolderCount());
      string[] Lit = Bomb.GetOnIndicators().ToArray();
      string[] Unlit = Bomb.GetOffIndicators().ToArray();
      Array.Sort(Lit);
      Array.Sort(Unlit);
      for (int L = 0; L < Lit.Length; L++) {
         DigitSequence += ("1" + ConvertIndicator(Lit[L]));
      }
      for (int U = 0; U < Unlit.Length; U++) {
         DigitSequence += ("0" + ConvertIndicator(Unlit[U]));
      }
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Lit indicators on the bomb: {1}", ModuleID, (Lit.Length == 0 ? "None" : Lit.Join(", ")));
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Unlit indicators on the bomb: {1}", ModuleID, (Unlit.Length == 0 ? "None" : Unlit.Join(", ")));
      string[][] IndividualPlates = Bomb.GetPortPlates().ToArray();
      int PlateCount = Bomb.GetPortPlateCount();
      string AllPlates = IndividualPlates.Select(i => i.Join(",")).Join(";");
      DigitSequence += ConvertPortPlates(AllPlates, PlateCount);
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Port plates on the bomb: {1}", ModuleID, (IndividualPlates.Select(i => i.Join(", ")).Join("; ")));
      string SerialNumber = Bomb.GetSerialNumber();
      DigitSequence += ConvertSerialNumber(SerialNumber);
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Serial Number on the bomb: {1}", ModuleID, SerialNumber);
      DigitSequence += (Number % 10).ToString();
      Debug.LogFormat("[Futuristic Combo Lock #{0}] Last digit on the module: {1}", ModuleID, Number % 10);
      Debug.LogFormat("[Futuristic Combo Lock #{0}] The long sequence of digits is {1}.", ModuleID, DigitSequence);
      Debug.LogFormat("[Futuristic Combo Lock #{0}] The back and forth conversion went as follows:", ModuleID);
      string ConvertedDigits = ConvertDigits(DigitSequence);
      Debug.LogFormat("[Futuristic Combo Lock #{0}] The final combination is {1}.", ModuleID, ConvertedDigits.Join(", "));
      CorrectSequence = FormInput(ConvertedDigits);
   }

   string ConvertIndicator (string I) {
      string S = "";
      string A = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      for (int C = 0; C < I.Length; C++) {
         S += A.IndexOf(I[C]).ToString();
      }
      return S;
   }

   string ConvertPortPlates (string P, int O) {
      if (O == 0) { return ""; }
      string[] X = { "Parallel,Serial", "Parallel", "Serial", "DVI,PS2,RJ45,StereoRCA", "DVI,PS2,StereoRCA", "PS2,RJ45,StereoRCA", "DVI,RJ45,StereoRCA", "DVI,PS2,RJ45", "PS2,StereoRCA", "DVI,StereoRCA", "RJ45,StereoRCA", "DVI,PS2", "PS2,RJ45", "DVI,RJ45", "StereoRCA", "PS2", "DVI", "RJ45", "" };
      string[] N = { "27", "18", "16", "49", "34", "36", "37", "35", "23", "25", "28", "21", "26", "24", "14", "12", "13", "15", "0" };
      string S = "";
      string[] A = P.Split(';');
      for (int C = 0; C < 19; C++) {
         for (int T = 0; T < A.Length; T++) {
            if (A[T] == X[C]) {
               S += N[C];
            }
         }
      }
      return S;
   }

   string ConvertSerialNumber (string N) {
      string S = "";
      string A = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      for (int C = 0; C < N.Length; C++) {
         if (A.Contains(N[C])) {
            S += A.IndexOf(N[C]).ToString();
         } else {
            S += N[C];
         }
      }
      return S;
   }

   string ConvertDigits (string D) { 
      string S = D;
      int I = 0;
      string N = "0123456789";
      bool A = true;
      string C = "";
      string[] M = { "0 1 2 8 9 ", " 1 2 3 9 0", "1 2 3 4 0 ", " 2 3 4 5 1", "2 3 4 5 6 ", " 3 4 5 6 7", "8 4 5 6 7 ", " 9 5 6 7 8", "9 0 6 7 8 ", " 0 1 7 8 9" };
      while (true) {
         for (int L = 0; L < S.Length-1; L++) {
            if ((N.IndexOf(S[L]) % 2) == (N.IndexOf(S[L+1]) % 2)) {
               A = false;
               break;
            }
         }
         if (A) {
            if (S.Length % 2 == 1) {
               C = S[S.Length / 2].ToString();
               S = S.Remove(S.Length / 2, 1);
               C = N[9 - N.IndexOf(C)].ToString();
               S = S.Insert(S.Length / 2, C);
               Debug.LogFormat("[Futuristic Combo Lock #{0}] • {1}", ModuleID, (I % 2 == 0 ? S : Reverse(S)));
            } else if (S.Length % 2 == 0) {
               C = S[S.Length / 2].ToString();
               S = S.Remove(S.Length / 2, 1);
               C = N[9 - N.IndexOf(C)].ToString();
               S = S.Insert((S.Length / 2) + 1, C);
               C = S[(S.Length / 2) - 1].ToString();
               S = S.Remove((S.Length / 2) - 1, 1);
               C = N[9 - N.IndexOf(C)].ToString();
               S = S.Insert(S.Length / 2, C);
               Debug.LogFormat("[Futuristic Combo Lock #{0}] • {1}", ModuleID, (I % 2 == 0 ? S : Reverse(S)));
            }
         }
         for (int P = 0; P < S.Length-1; P++) {
            if ((N.IndexOf(S[P]) % 2) == (N.IndexOf(S[P + 1]) % 2)) {
               C = S.Substring(P, 2);
               S = S.Remove(P, 2);
               S = S.Insert(P, M[N.IndexOf(C[0])][N.IndexOf(C[1])].ToString());
               Debug.LogFormat("[Futuristic Combo Lock #{0}] • {1}", ModuleID, ((I % 2 == 0) ? S : Reverse(S)));
               if (S.Length == 5) {
                  return ((I % 2 == 0) ? S : Reverse(S));
               }
            }
         }
         S = Reverse(S);
         I++;
         A = true;
      }
   }

   string Reverse (string S) {
      char[] A = S.ToCharArray();
      Array.Reverse(A);
      return A.Join("");
   }

   string FormInput (string S) {
      string O = "";
      string N = "0123456789";
      int D;
      bool T = false;
      O = S[0].ToString();
      D = N.IndexOf(O);
      for (int F = 0; F < 2; F++) {
         for (int R = 0; R < 10; R++) {
            if (T) { continue; }
            D = (D + 1) % 10;
            O += D.ToString();
            if (D.ToString() == S[F * 2 + 1].ToString()) {
               T = true;
            }
         }
         T = false;
         for (int L = 0; L < 10; L++) {
            if (T) { continue; }
            D = (D + 9) % 10;
            O += D.ToString();
            if (D.ToString() == S[F * 2 + 2].ToString()) {
               T = true;
            }
         }
         T = false;
      }
      return O;
   }

   void WedgePress (KMSelectable W) {
      if (Inputting || ModuleSolved) { return; }
      for (int P = 0; P < 10; P++) {
         if (Wedges[P] == W) {
            StopAllCoroutines();
            Inputting = true;
            GivenSequence += P.ToString();
            Audio.PlaySoundAtTransform("dial_click", transform);
            WedgeMeshes[P].material = Materials[19];
            Shading[P] = 9;
         }
      }
   }

   void WedgeHover (KMSelectable W) {
      if (!Inputting || ModuleSolved) { return; }
      for (int H = 0; H < 10; H++) {
         if (Wedges[H] == W) {
            GivenSequence += H.ToString();
            Audio.PlaySoundAtTransform("dial_click", transform);
            WedgeMeshes[H].material = Materials[19];
            Shading[H] = 9;
            for (int C = 0; C < 10; C++) {
               if (C == H || Shading[C] == -1) { continue; }
               Shading[C]--;
               WedgeMeshes[C].material = Materials[10+Shading[C]];
            }
         }
      }
   }

   void Submit() {
      if (GivenSequence == "" || ModuleSolved) { return; }
      Inputting = false;
      if (GivenSequence != CorrectSequence) {
         Debug.LogFormat("<Futuristic Combo Lock #{0}> Incorrect submission: {1}", ModuleID, GivenSequence);
         Audio.PlaySoundAtTransform("dial_reset", transform);
         GivenSequence = "";
         StartCoroutine(Spin());
      } else {
         Debug.LogFormat("[Futuristic Combo Lock #{0}] Correct combination submitted. Module solved.", ModuleID);
         Audio.PlaySoundAtTransform("unlock", transform);
         GetComponent<KMBombModule>().HandlePass();
         ModuleSolved = true;
         StartCoroutine(Darken(CorrectSequence[CorrectSequence.Length-1]));
      }
   }

   private IEnumerator Spin() {
      int R = 0;
      while (!Inputting) {
         yield return new WaitForSeconds(0.1f);
         R = (R + 9) % 10;
         for (int W = 0; W < 10; W++) {
            WedgeMeshes[W].material = Materials[(W+R)%10];
         }
      }
   }

   private IEnumerator Darken(char P) {
      for (int S = 0; S < 10; S++) {
         Shading[S] = -1;
      }
      string N = "0123456789";
      int I = N.IndexOf(P);
      Shading[I] = 0;
      for (int D = 0; D < 18; D++) {
         yield return new WaitForSeconds(0.05f);
         Digits.color = new Color32((byte)(255 - (15 * D)), 0, 0, 255);
         if (D == 0) { continue; }
         for (int W = 0; W < 10; W++) {
            if (Shading[W] == 9) { continue; }
            if (Shading[W] != -1) { Shading[W]++; }
            if (D < 6 && (W == ((I + D) % 10) || W == ((I + (10 - D)) % 10))) {
               Shading[W] = 0;
            }
            WedgeMeshes[W].material = Materials[10+Shading[W]];
         }
      }
   }
}
