"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.JavaScriptLanguageGenerator = exports.JavaScriptFormatter = void 0;
var _language = require("./language");
var _utils = require("./utils");
var _deviceDescriptors = _interopRequireDefault(require("../deviceDescriptors"));
var _stringUtils = require("../../utils/isomorphic/stringUtils");
var _locatorGenerators = require("../isomorphic/locatorGenerators");
function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }
/**
 * Copyright (c) Microsoft Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

class JavaScriptLanguageGenerator {
  constructor(isTest) {
    this.id = void 0;
    this.groupName = 'Node.js';
    this.name = void 0;
    this.highlighter = 'javascript';
    this._isTest = void 0;
    this.id = isTest ? 'playwright-test' : 'javascript';
    this.name = isTest ? 'Test Runner' : 'Library';
    this._isTest = isTest;
  }
  generateAction(actionInContext) {
    const action = actionInContext.action;
    if (this._isTest && (action.name === 'openPage' || action.name === 'closePage')) return '';
    const pageAlias = actionInContext.frame.pageAlias;
    const formatter = new JavaScriptFormatter(2);
    if (action.name === 'openPage') {
      formatter.add(`const ${pageAlias} = await context.newPage();`);
      if (action.url && action.url !== 'about:blank' && action.url !== 'chrome://newtab/') formatter.add(`await ${pageAlias}.goto(${quote(action.url)});`);
      return formatter.format();
    }
    let subject;
    if (actionInContext.frame.isMainFrame) {
      subject = pageAlias;
    } else if (actionInContext.frame.selectorsChain && action.name !== 'navigate') {
      const locators = actionInContext.frame.selectorsChain.map(selector => `.frameLocator(${quote(selector)})`);
      subject = `${pageAlias}${locators.join('')}`;
    } else if (actionInContext.frame.name) {
      subject = `${pageAlias}.frame(${formatObject({
        name: actionInContext.frame.name
      })})`;
    } else {
      subject = `${pageAlias}.frame(${formatObject({
        url: actionInContext.frame.url
      })})`;
    }
    const signals = (0, _language.toSignalMap)(action);
    if (signals.dialog) {
      formatter.add(`  ${pageAlias}.once('dialog', dialog => {
    console.log(\`Dialog message: $\{dialog.message()}\`);
    dialog.dismiss().catch(() => {});
  });`);
    }
    const emitPromiseAll = signals.popup || signals.download;
    if (emitPromiseAll) {
      // Generate either await Promise.all([]) or
      // const [popup1] = await Promise.all([]).
      let leftHandSide = '';
      if (signals.popup) leftHandSide = `const [${signals.popup.popupAlias}] = `;else if (signals.download) leftHandSide = `const [download] = `;
      formatter.add(`${leftHandSide}await Promise.all([`);
    }

    // Popup signals.
    if (signals.popup) formatter.add(`${pageAlias}.waitForEvent('popup'),`);

    // Download signals.
    if (signals.download) formatter.add(`${pageAlias}.waitForEvent('download'),`);
    const prefix = signals.popup || signals.download ? '' : 'await ';
    const actionCall = this._generateActionCall(action);
    const suffix = emitPromiseAll ? '' : ';';
    formatter.add(`${prefix}${subject}.${actionCall}${suffix}`);
    if (emitPromiseAll) formatter.add(`]);`);
    return formatter.format();
  }
  _generateActionCall(action) {
    switch (action.name) {
      case 'openPage':
        throw Error('Not reached');
      case 'closePage':
        return 'close()';
      case 'click':
        {
          let method = 'click';
          if (action.clickCount === 2) method = 'dblclick';
          const modifiers = (0, _utils.toModifiers)(action.modifiers);
          const options = {};
          if (action.button !== 'left') options.button = action.button;
          if (modifiers.length) options.modifiers = modifiers;
          if (action.clickCount > 2) options.clickCount = action.clickCount;
          if (action.position) options.position = action.position;
          const optionsString = formatOptions(options, false);
          return this._asLocator(action.selector) + `.${method}(${optionsString})`;
        }
      case 'check':
        return this._asLocator(action.selector) + `.check()`;
      case 'uncheck':
        return this._asLocator(action.selector) + `.uncheck()`;
      case 'fill':
        return this._asLocator(action.selector) + `.fill(${quote(action.text)})`;
      case 'setInputFiles':
        return this._asLocator(action.selector) + `.setInputFiles(${formatObject(action.files.length === 1 ? action.files[0] : action.files)})`;
      case 'press':
        {
          const modifiers = (0, _utils.toModifiers)(action.modifiers);
          const shortcut = [...modifiers, action.key].join('+');
          return this._asLocator(action.selector) + `.press(${quote(shortcut)})`;
        }
      case 'navigate':
        return `goto(${quote(action.url)})`;
      case 'select':
        return this._asLocator(action.selector) + `.selectOption(${formatObject(action.options.length > 1 ? action.options : action.options[0])})`;
    }
  }
  _asLocator(selector) {
    return (0, _locatorGenerators.asLocator)('javascript', selector);
  }
  generateHeader(options) {
    if (this._isTest) return this.generateTestHeader(options);
    return this.generateStandaloneHeader(options);
  }
  generateFooter(saveStorage) {
    if (this._isTest) return this.generateTestFooter(saveStorage);
    return this.generateStandaloneFooter(saveStorage);
  }
  generateTestHeader(options) {
    const formatter = new JavaScriptFormatter();
    const useText = formatContextOptions(options.contextOptions, options.deviceName);
    formatter.add(`
      import { test, expect${options.deviceName ? ', devices' : ''} } from '@playwright/test';
${useText ? '\ntest.use(' + useText + ');\n' : ''}
      test('test', async ({ page }) => {`);
    return formatter.format();
  }
  generateTestFooter(saveStorage) {
    return `});`;
  }
  generateStandaloneHeader(options) {
    const formatter = new JavaScriptFormatter();
    formatter.add(`
      const { ${options.browserName}${options.deviceName ? ', devices' : ''} } = require('playwright');

      (async () => {
        const browser = await ${options.browserName}.launch(${formatObjectOrVoid(options.launchOptions)});
        const context = await browser.newContext(${formatContextOptions(options.contextOptions, options.deviceName)});`);
    return formatter.format();
  }
  generateStandaloneFooter(saveStorage) {
    const storageStateLine = saveStorage ? `\n  await context.storageState({ path: ${quote(saveStorage)} });` : '';
    return `\n  // ---------------------${storageStateLine}
  await context.close();
  await browser.close();
})();`;
  }
}
exports.JavaScriptLanguageGenerator = JavaScriptLanguageGenerator;
function formatOptions(value, hasArguments) {
  const keys = Object.keys(value);
  if (!keys.length) return '';
  return (hasArguments ? ', ' : '') + formatObject(value);
}
function formatObject(value, indent = '  ') {
  if (typeof value === 'string') return quote(value);
  if (Array.isArray(value)) return `[${value.map(o => formatObject(o)).join(', ')}]`;
  if (typeof value === 'object') {
    const keys = Object.keys(value).filter(key => value[key] !== undefined).sort();
    if (!keys.length) return '{}';
    const tokens = [];
    for (const key of keys) tokens.push(`${key}: ${formatObject(value[key])}`);
    return `{\n${indent}${tokens.join(`,\n${indent}`)}\n}`;
  }
  return String(value);
}
function formatObjectOrVoid(value, indent = '  ') {
  const result = formatObject(value, indent);
  return result === '{}' ? '' : result;
}
function formatContextOptions(options, deviceName) {
  const device = deviceName && _deviceDescriptors.default[deviceName];
  if (!device) return formatObjectOrVoid(options);
  // Filter out all the properties from the device descriptor.
  let serializedObject = formatObjectOrVoid((0, _language.sanitizeDeviceOptions)(device, options));
  // When there are no additional context options, we still want to spread the device inside.
  if (!serializedObject) serializedObject = '{\n}';
  const lines = serializedObject.split('\n');
  lines.splice(1, 0, `...devices[${quote(deviceName)}],`);
  return lines.join('\n');
}
class JavaScriptFormatter {
  constructor(offset = 0) {
    this._baseIndent = void 0;
    this._baseOffset = void 0;
    this._lines = [];
    this._baseIndent = ' '.repeat(2);
    this._baseOffset = ' '.repeat(offset);
  }
  prepend(text) {
    this._lines = text.trim().split('\n').map(line => line.trim()).concat(this._lines);
  }
  add(text) {
    this._lines.push(...text.trim().split('\n').map(line => line.trim()));
  }
  newLine() {
    this._lines.push('');
  }
  format() {
    let spaces = '';
    let previousLine = '';
    return this._lines.map(line => {
      if (line === '') return line;
      if (line.startsWith('}') || line.startsWith(']')) spaces = spaces.substring(this._baseIndent.length);
      const extraSpaces = /^(for|while|if|try).*\(.*\)$/.test(previousLine) ? this._baseIndent : '';
      previousLine = line;
      const callCarryOver = line.startsWith('.set');
      line = spaces + extraSpaces + (callCarryOver ? this._baseIndent : '') + line;
      if (line.endsWith('{') || line.endsWith('[')) spaces += this._baseIndent;
      return this._baseOffset + line;
    }).join('\n');
  }
}
exports.JavaScriptFormatter = JavaScriptFormatter;
function quote(text) {
  return (0, _stringUtils.escapeWithQuotes)(text, '\'');
}