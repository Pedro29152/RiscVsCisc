using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

namespace RiscVsCisc
{
    public class Cisc
    {
        private const int INITIAL_DELAY = 1000;
        private const int ACTION_DELAY = 100;

        public static double MakeOperation(double n1, double n2, Operations op)
        {
            string operation = "";

            switch (op)
            {
                case Operations.Sum:
                    operation = n1 + "+" + n2;
                    break;
                case Operations.Subtract:
                    operation = n1 + "-" + n2;
                    break;
                case Operations.Divide:
                    operation = n1 + "/" + n2;
                    break;
                case Operations.Multiply:
                    operation = n1 + "*" + n2;
                    break;

                default:
                    throw new ArgumentException("Unknown operation");
            }

            return MakeOperation(operation);
        }

        public static double MakeOperation(string operation)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "calc.exe";
            psi.WorkingDirectory = @"C:\Windows\System32";
            Process calcNew = Process.Start(psi);

            Thread.Sleep(INITIAL_DELAY);

            Process calcHostProcess = Process.GetProcessesByName("ApplicationFrameHost")?[0];
            AutomationElement aeCalc = AutomationElement.FromHandle(calcHostProcess.MainWindowHandle);
            aeCalc.SetFocus();

            CalculatorFields fields = new CalculatorFields();
            if (GetAutomationElements(fields.StandardCalculatorMode, aeCalc)?[0] != null)
            {
                AutomationElement navigationButton = GetAutomationElements(fields.NavigationButton, aeCalc)?[0];
                var clickPointMenu = navigationButton.GetClickablePoint();
                MouseInput.LeftClick(clickPointMenu.X, clickPointMenu.Y);

                Thread.Sleep(ACTION_DELAY);

                AutomationElement comumButton = GetAutomationElements(fields.ScientificCalculatorMenuButton, aeCalc)?[0];
                var clickPointStandard = comumButton.GetClickablePoint();
                MouseInput.LeftClick(clickPointStandard.X, clickPointStandard.Y);

                Thread.Sleep(ACTION_DELAY);
            }

            SendKeys.SendWait(operation);
            SendKeys.SendWait("{ENTER}");

            TreeWalker walker = TreeWalker.RawViewWalker;
            AutomationElement resultPane = GetAutomationElements(fields.ValueDisplayPrefix, aeCalc, likeMode: true)?[0];
            AutomationElement child = walker.GetFirstChild(resultPane);
            string value = child.Current.Name;

            double result = double.Parse(value);

            SendKeys.SendWait("%{F4}");

            return result;
        }

        private static IList<AutomationElement> GetAutomationElements(string name, AutomationElement rootElement = null, int treeHeight = 0, bool likeMode = false)
        {
            var elements = _GetAutomationElements(name, rootElement, treeHeight, likeMode, 1);

            if (elements.Count < 1) return null;
            return elements;
        }

        private static IList<AutomationElement> _GetAutomationElements(string name, AutomationElement rootElement = null, int treeHeight = 0, bool likeMode = false, int currentTreeHeight = 1)
        {
            TreeWalker walker = GetTreeWalker();

            AutomationElement elementNode = walker.GetFirstChild(rootElement == null ? AutomationElement.RootElement : rootElement);

            List<AutomationElement> elements = new List<AutomationElement>();
            while (elementNode != null && (treeHeight > 0 ? currentTreeHeight < treeHeight : true))
            {
                string currentName = elementNode.Current.Name;
                currentName = currentName == null ? string.Empty : currentName;

                if (!likeMode)
                {
                    if (currentName == name)
                    {
                        elements.Add(elementNode);
                    }
                }
                else
                {
                    for (int i = 0; i <= currentName.Length - name.Length; i++)
                    {
                        if (currentName.Substring(i, name.Length) == name)
                        {
                            elements.Add(elementNode);
                            break;
                        }
                    }
                }

                var ae = _GetAutomationElements(name, elementNode, treeHeight, likeMode, currentTreeHeight + 1);
                elements.AddRange(ae);
                elementNode = walker.GetNextSibling(elementNode);
            }

            return elements;
        }

        private static TreeWalker GetTreeWalker()
        {
            Condition c1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition c2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);

            Condition cFinal = new AndCondition(c1, c2);

            return new TreeWalker(cFinal);
        }
    }

    class MouseInput
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtrainfo);

        private enum MouseEvents
        {
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010
        };
        public static void LeftClick()
        {
            mouse_event((int)MouseEvents.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event((int)MouseEvents.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        public static void LeftClick(int x, int y)
        {
            Cursor.Position = new Point(x, y);
            LeftClick();
        }
        public static void LeftClick(double x, double y)
        {
            LeftClick((int)x, (int)y);
        }
    }

    class CalculatorFields
    {
        public readonly CultureInfo CurrentCulture;

        public readonly string NavigationButton;
        public readonly string ScientificCalculatorMode;
        public readonly string StandardCalculatorMode;
        public readonly string ScientificCalculatorMenuButton;
        public readonly string StandardCalculatorMenuButton;
        public readonly string ValueDisplayPrefix;

        public CalculatorFields()
        {
            CurrentCulture = Thread.CurrentThread.CurrentUICulture;

            var enUs = new CultureInfo("en-US");
            var ptBR = new CultureInfo("pt-BR");

            if (CurrentCulture.Name == enUs.Name)
            {
                NavigationButton = "Open Navigation";
                StandardCalculatorMode = "Standard Calculator Mode";
                ScientificCalculatorMode = "Scientific Calculator Mode";
                ScientificCalculatorMenuButton = "Scientific Calculator";
                StandardCalculatorMenuButton = "Standard Calculator";
                ValueDisplayPrefix = "Display is ";
            }
            else if (CurrentCulture.Name == ptBR.Name)
            {
                NavigationButton = "Abrir Navegação";
                StandardCalculatorMode = "Modo Calculadora Padrão";
                ScientificCalculatorMode = "Modo Calculadora Científica";
                ScientificCalculatorMenuButton = "Científica Calculadora";
                StandardCalculatorMenuButton = "Padrão Calculadora";
                ValueDisplayPrefix = "A exibição é ";
            }
            else
            {
                NavigationButton = "InsertNavigationButtonLabel";
                StandardCalculatorMode = "InsertStandardCalculatorModeLabel";
                ScientificCalculatorMode = "InserScientificCalculatorModeLabel";
                ScientificCalculatorMenuButton = "InsertScientificCalculatorMenuButtonLabel";
                StandardCalculatorMenuButton = "InsertStandardCalculatorMenuButtonLabel";
                ValueDisplayPrefix = "InsertValueDisplayPrefixLabel";
            }
        }
    }

}
