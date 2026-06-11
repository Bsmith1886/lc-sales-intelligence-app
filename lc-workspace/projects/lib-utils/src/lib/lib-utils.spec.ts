import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LibUtils } from './lib-utils';

describe('LibUtils', () => {
  let component: LibUtils;
  let fixture: ComponentFixture<LibUtils>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LibUtils],
    }).compileComponents();

    fixture = TestBed.createComponent(LibUtils);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
