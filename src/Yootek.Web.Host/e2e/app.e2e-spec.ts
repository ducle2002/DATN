import { YootekTemplatePage } from './app.po';

describe('Yootek App', function() {
  let page: YootekTemplatePage;

  beforeEach(() => {
    page = new YootekTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
